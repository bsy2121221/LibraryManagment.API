using LibraryManagement.DataAccess.IRepositories;
using LibraryManagement.Models.DTOs.Bookings;
using LibraryManagement.Models.DTOs.Students;
using LibraryManagement.Models.Enums;
using LibraryManagement.Models.Exceptions;
using LibraryManagement.Models.Options;
using LibraryManagement.Services.IServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LibraryManagement.Services.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly ISettingsService _settingsService;
        private readonly TimeProvider _timeProvider;
        private readonly PaymentOptions _paymentOptions;
        private readonly ILogger<BookingService> _logger;

        public BookingService(
            IBookingRepository bookingRepository,
            ISettingsService settingsService,
            TimeProvider timeProvider,
            IOptions<PaymentOptions> paymentOptions,
            ILogger<BookingService> logger)
        {
            _bookingRepository = bookingRepository;
            _settingsService = settingsService;
            _timeProvider = timeProvider;
            _paymentOptions = paymentOptions.Value;
            _logger = logger;
        }

        private DateTime UtcNow => _timeProvider.GetUtcNow().UtcDateTime;

        public async Task<BookingDto> HoldSeatAsync(int userId, int seatId, CancellationToken cancellationToken = default)
        {
            var holdMinutes = await _settingsService.GetSeatHoldMinutesAsync(cancellationToken);
            var booking = await _bookingRepository.HoldAsync(userId, seatId, holdMinutes, cancellationToken);

            _logger.LogInformation("User {UserId} holds seat {SeatId} (booking {BookingId}) until {HoldExpiresAt}",
                userId, seatId, booking.Id, booking.HoldExpiresAt);
            return BookingDto.FromEntity(booking, UtcNow);
        }

        public async Task<BookingDto?> GetMyBookingAsync(int userId, CancellationToken cancellationToken = default)
        {
            var booking = await _bookingRepository.GetLiveByUserAsync(userId, cancellationToken);
            return booking is null ? null : BookingDto.FromEntity(booking, UtcNow);
        }

        public async Task CancelHoldAsync(int userId, int bookingId, CancellationToken cancellationToken = default)
        {
            await _bookingRepository.CancelHoldAsync(bookingId, userId, cancellationToken);
            _logger.LogInformation("User {UserId} cancelled hold {BookingId}", userId, bookingId);
        }

        public async Task<IReadOnlyList<BookingDto>> GetAllAsync(BookingStatus? status, string? search, CancellationToken cancellationToken = default)
        {
            var bookings = await _bookingRepository.GetAllForAdminAsync(status, search?.Trim(), cancellationToken);
            var now = UtcNow;
            return bookings.Select(b => BookingDto.FromEntity(b, now)).ToList();
        }

        public async Task<BookingDto> AssignSeatAsync(int adminUserId, int studentId, AssignSeatRequest request, CancellationToken cancellationToken = default)
        {
            var today = LibraryClock.Today(_timeProvider);
            var startDate = request.StartDate ?? today;
            if (startDate < today.AddYears(-1) || startDate > today.AddDays(60))
            {
                throw new BadRequestException("The start date must be within the last year and at most 60 days ahead.");
            }

            var amount = request.Amount ?? await _settingsService.GetMonthlyFeeAsync(cancellationToken);
            var note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();

            var bookingId = await _bookingRepository.AssignByAdminAsync(studentId, request.SeatId, startDate, amount, request.PaidAtDesk,
                note, adminUserId, _paymentOptions.Currency, cancellationToken);

            _logger.LogInformation("Admin {AdminUserId} assigned seat {SeatId} to student {StudentId} from {StartDate} (booking {BookingId}, paid at desk: {Paid})",
                adminUserId, request.SeatId, studentId, startDate, bookingId, request.PaidAtDesk);

            var booking = await _bookingRepository.GetByIdAsync(bookingId, cancellationToken)
                ?? throw new NotFoundException("Booking not found.");
            return BookingDto.FromEntity(booking, UtcNow);
        }

        public async Task ReleaseAsync(int adminUserId, int bookingId, string reason, CancellationToken cancellationToken = default)
        {
            await _bookingRepository.ReleaseAsync(bookingId, adminUserId, reason.Trim(), cancellationToken);
            _logger.LogInformation("Admin {AdminUserId} released booking {BookingId}: {Reason}", adminUserId, bookingId, reason);
        }

        public async Task<BookingDto> MoveSeatAsync(int adminUserId, int bookingId, MoveSeatRequest request, CancellationToken cancellationToken = default)
        {
            var reason = string.IsNullOrWhiteSpace(request.Reason) ? "Moved by admin" : request.Reason.Trim();
            await _bookingRepository.MoveSeatAsync(bookingId, request.ToSeatId, adminUserId, reason, cancellationToken);

            _logger.LogInformation("Admin {AdminUserId} moved booking {BookingId} to seat {SeatId}", adminUserId, bookingId, request.ToSeatId);
            var booking = await _bookingRepository.GetByIdAsync(bookingId, cancellationToken)
                ?? throw new NotFoundException("Booking not found.");
            return BookingDto.FromEntity(booking, UtcNow);
        }
    }
}
