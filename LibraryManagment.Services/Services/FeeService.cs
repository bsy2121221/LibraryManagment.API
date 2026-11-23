using LibraryManagement.DataAccess.IRepositories;
using LibraryManagement.Models.DTOs.Fees;
using LibraryManagement.Models.Enums;
using LibraryManagement.Models.Exceptions;
using LibraryManagement.Models.Options;
using LibraryManagement.Services.IServices;
using Microsoft.Extensions.Logging;
using LibraryManagement.Services.Notifications;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace LibraryManagement.Services.Services
{
    // Monthly fees. Invoices are created by the billing job; students pay online (PaymentService),
    // or the admin records a payment made at the desk.
    public class FeeService : IFeeService
    {
        private readonly IFeeRepository _feeRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly ISettingsService _settingsService;
        private readonly TimeProvider _timeProvider;
        private readonly PaymentOptions _paymentOptions;
        private readonly ILogger<FeeService> _logger;

        public FeeService(
            IFeeRepository feeRepository,
            IBookingRepository bookingRepository,
            ISettingsService settingsService,
            TimeProvider timeProvider,
            IOptions<PaymentOptions> paymentOptions,
            ILogger<FeeService> logger)
        {
            _feeRepository = feeRepository;
            _bookingRepository = bookingRepository;
            _settingsService = settingsService;
            _timeProvider = timeProvider;
            _paymentOptions = paymentOptions.Value;
            _logger = logger;
        }

        public async Task<IReadOnlyList<FeeInvoiceDto>> GetMyFeesAsync(int userId, CancellationToken cancellationToken = default)
        {
            var fees = await _feeRepository.GetByUserAsync(userId, cancellationToken);
            return fees.Select(FeeInvoiceDto.FromEntity).ToList();
        }

        public async Task<OverdueNoticeDto?> GetMyOverdueNoticeAsync(int userId, CancellationToken cancellationToken = default)
        {
            var overdue = (await _feeRepository.GetByUserAsync(userId, cancellationToken))
                .Where(f => f.Status == InvoiceStatus.Overdue)
                .OrderBy(f => f.DueDate)
                .Select(FeeInvoiceDto.FromEntity)
                .ToList();
            if (overdue.Count == 0)
            {
                return null;
            }

            var (libraryName, title, message) = await _settingsService.GetOverdueNoticeAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(message))
            {
                return null;
            }

            var oldest = overdue[0];
            var total = overdue.Sum(f => f.Amount);
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Name"] = oldest.StudentName,
                ["LibraryName"] = libraryName,
                ["Amount"] = total.ToString("0.##", CultureInfo.InvariantCulture),
                ["DueDate"] = oldest.DueDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture),
                ["DaysOverdue"] = Math.Max(0, LibraryClock.Today(_timeProvider).DayNumber - oldest.DueDate.DayNumber).ToString(CultureInfo.InvariantCulture),
                ["Count"] = overdue.Count.ToString(CultureInfo.InvariantCulture),
                ["Seat"] = oldest.SeatLabel,
                ["Section"] = oldest.SectionName
            };

            return new OverdueNoticeDto
            {
                Title = TemplateRenderer.Render(title, values),
                Message = TemplateRenderer.Render(message, values),
                TotalAmount = total,
                Fees = overdue
            };
        }

        public async Task<IReadOnlyList<FeeInvoiceDto>> GetAllAsync(InvoiceStatus? status, string? search, CancellationToken cancellationToken = default)
        {
            var fees = await _feeRepository.GetForAdminAsync(status, search?.Trim(), cancellationToken);
            return fees.Select(FeeInvoiceDto.FromEntity).ToList();
        }

        public async Task<FeeInvoiceDto> CreateAsync(int adminUserId, CreateFeeRequest request, CancellationToken cancellationToken = default)
        {
            var booking = await _bookingRepository.GetLiveByUserAsync(request.StudentId, cancellationToken);
            if (booking is null || booking.Status != BookingStatus.Active)
            {
                throw new ConflictException("This student has no seat. Assign a seat first; fees belong to a seat.");
            }

            var periodStart = request.PeriodStart!.Value;
            var periodEnd = request.PeriodEnd ?? periodStart.AddMonths(1).AddDays(-1);
            var dueDate = request.DueDate ?? periodStart;
            var today = LibraryClock.Today(_timeProvider);

            if (periodEnd < periodStart)
            {
                throw new BadRequestException("The period end must be on or after the period start.");
            }
            if (periodEnd > periodStart.AddYears(1))
            {
                throw new BadRequestException("A fee can cover at most one year.");
            }
            if (periodStart < today.AddYears(-2) || periodStart > today.AddYears(1))
            {
                throw new BadRequestException("The period must start within the last two years and at most one year ahead.");
            }

            var amount = request.Amount ?? await _settingsService.GetMonthlyFeeAsync(cancellationToken);
            var invoiceId = await _feeRepository.CreateByAdminAsync(booking.Id, periodStart, periodEnd, amount, dueDate, request.PaidAtDesk,
                string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(), adminUserId, _paymentOptions.Currency, cancellationToken);

            _logger.LogInformation("Admin {AdminUserId} added fee {InvoiceId} for student {StudentId}: {PeriodStart}..{PeriodEnd}, {Amount}, paid at desk: {Paid}",
                adminUserId, invoiceId, request.StudentId, periodStart, periodEnd, amount, request.PaidAtDesk);

            var invoice = await _feeRepository.GetByIdAsync(invoiceId, cancellationToken)
                ?? throw new NotFoundException("Fee not found.");
            return FeeInvoiceDto.FromEntity(invoice);
        }

        public async Task<FeeInvoiceDto> MarkPaidAsync(int adminUserId, int invoiceId, string? note, CancellationToken cancellationToken = default)
        {
            await _feeRepository.MarkPaidAsync(invoiceId, adminUserId, string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
                _paymentOptions.Currency, cancellationToken);
            _logger.LogInformation("Admin {AdminUserId} recorded fee {InvoiceId} as paid at the desk", adminUserId, invoiceId);

            var invoice = await _feeRepository.GetByIdAsync(invoiceId, cancellationToken)
                ?? throw new NotFoundException("Fee not found.");
            return FeeInvoiceDto.FromEntity(invoice);
        }
    }
}
