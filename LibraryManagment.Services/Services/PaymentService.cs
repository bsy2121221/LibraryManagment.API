using LibraryManagement.DataAccess.IRepositories;
using LibraryManagement.Models.DTOs.Bookings;
using LibraryManagement.Models.DTOs.Payments;
using LibraryManagement.Models.Entities;
using LibraryManagement.Models.Enums;
using LibraryManagement.Models.Exceptions;
using LibraryManagement.Models.Options;
using LibraryManagement.Services.IServices;
using LibraryManagement.Services.Payments;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace LibraryManagement.Services.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentGateway _gateway;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IUserRepository _userRepository;
        private readonly IFeeRepository _feeRepository;
        private readonly ISettingsService _settingsService;
        private readonly PaymentOptions _options;
        private readonly TimeProvider _timeProvider;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IPaymentGateway gateway,
            IPaymentRepository paymentRepository,
            IBookingRepository bookingRepository,
            IUserRepository userRepository,
            IFeeRepository feeRepository,
            ISettingsService settingsService,
            IOptions<PaymentOptions> options,
            TimeProvider timeProvider,
            ILogger<PaymentService> logger)
        {
            _gateway = gateway;
            _paymentRepository = paymentRepository;
            _bookingRepository = bookingRepository;
            _userRepository = userRepository;
            _feeRepository = feeRepository;
            _settingsService = settingsService;
            _options = options.Value;
            _timeProvider = timeProvider;
            _logger = logger;
        }

        private DateTime UtcNow => _timeProvider.GetUtcNow().UtcDateTime;

        public async Task<PaymentOrderDto> CreateBookingOrderAsync(int userId, int bookingId, CancellationToken cancellationToken = default)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId, cancellationToken);
            if (booking is null || booking.UserId != userId)
            {
                throw new NotFoundException("Booking not found.");
            }
            if (booking.Status == BookingStatus.Active)
            {
                throw new ConflictException("This seat is already booked and paid.");
            }
            if (!booking.IsHoldActive(UtcNow))
            {
                throw new ConflictException("Your seat hold has ended. Please select the seat again.");
            }

            var fee = await _settingsService.GetMonthlyFeeAsync(cancellationToken);
            var libraryName = await _settingsService.GetLibraryNameAsync(cancellationToken);
            var amountInPaise = (long)Math.Round(fee * 100, MidpointRounding.AwayFromZero);

            var order = await _gateway.CreateOrderAsync(amountInPaise, _options.Currency, $"booking-{booking.Id}",
                new Dictionary<string, string>
                {
                    ["bookingId"] = booking.Id.ToString(),
                    ["seat"] = $"{booking.SectionName} - {booking.SeatLabel}"
                },
                cancellationToken);

            await _paymentRepository.CreateAsync(new Payment
            {
                UserId = userId,
                BookingId = booking.Id,
                Gateway = _gateway.Name,
                GatewayOrderId = order.OrderId,
                Amount = fee,
                Currency = order.Currency
            }, cancellationToken);

            _logger.LogInformation("Payment order {OrderId} ({Gateway}) created for booking {BookingId}, amount {Amount}",
                order.OrderId, _gateway.Name, booking.Id, fee);

            return new PaymentOrderDto
            {
                Provider = _gateway.Name,
                KeyId = _gateway.PublicKey,
                OrderId = order.OrderId,
                AmountInPaise = order.AmountInPaise,
                Amount = fee,
                Currency = order.Currency,
                LibraryName = libraryName,
                Description = $"Seat {booking.SeatLabel} ({booking.SectionName}) - first month",
                Prefill = new PaymentPrefillDto
                {
                    Name = booking.StudentName,
                    Contact = booking.StudentMobile,
                    Email = booking.StudentEmail
                },
                Booking = BookingDto.FromEntity(booking, UtcNow)
            };
        }

        public async Task<PaymentOrderDto> CreateFeeOrderAsync(int userId, int invoiceId, CancellationToken cancellationToken = default)
        {
            var invoice = await _feeRepository.GetByIdAsync(invoiceId, cancellationToken);
            if (invoice is null || invoice.UserId != userId)
            {
                throw new NotFoundException("Fee not found.");
            }
            if (invoice.Status is not (InvoiceStatus.Due or InvoiceStatus.Overdue))
            {
                throw new ConflictException("This fee is already paid or was cancelled.");
            }

            var booking = await _bookingRepository.GetByIdAsync(invoice.BookingId, cancellationToken)
                ?? throw new NotFoundException("Booking not found.");
            var libraryName = await _settingsService.GetLibraryNameAsync(cancellationToken);
            var amountInPaise = (long)Math.Round(invoice.Amount * 100, MidpointRounding.AwayFromZero);

            var order = await _gateway.CreateOrderAsync(amountInPaise, _options.Currency, $"fee-{invoice.Id}",
                new Dictionary<string, string>
                {
                    ["feeInvoiceId"] = invoice.Id.ToString(),
                    ["seat"] = $"{invoice.SectionName} - {invoice.SeatLabel}"
                },
                cancellationToken);

            await _paymentRepository.CreateAsync(new Payment
            {
                UserId = userId,
                BookingId = invoice.BookingId,
                FeeInvoiceId = invoice.Id,
                Gateway = _gateway.Name,
                GatewayOrderId = order.OrderId,
                Amount = invoice.Amount,
                Currency = order.Currency
            }, cancellationToken);

            _logger.LogInformation("Payment order {OrderId} ({Gateway}) created for fee {InvoiceId}, amount {Amount}",
                order.OrderId, _gateway.Name, invoice.Id, invoice.Amount);

            return new PaymentOrderDto
            {
                Provider = _gateway.Name,
                KeyId = _gateway.PublicKey,
                OrderId = order.OrderId,
                AmountInPaise = order.AmountInPaise,
                Amount = invoice.Amount,
                Currency = order.Currency,
                LibraryName = libraryName,
                Description = $"Seat {invoice.SeatLabel} - fee {invoice.PeriodStart:dd MMM} to {invoice.PeriodEnd:dd MMM yyyy}",
                Prefill = new PaymentPrefillDto
                {
                    Name = booking.StudentName,
                    Contact = booking.StudentMobile,
                    Email = booking.StudentEmail
                },
                Booking = BookingDto.FromEntity(booking, UtcNow)
            };
        }

        public async Task<PaymentResultDto> VerifyAsync(int userId, VerifyPaymentRequest request, CancellationToken cancellationToken = default)
        {
            var payment = await GetOwnPaymentAsync(userId, request.OrderId, cancellationToken);

            if (!_gateway.VerifyPaymentSignature(request.OrderId, request.PaymentId, request.Signature))
            {
                _logger.LogWarning("Invalid payment signature for order {OrderId} from user {UserId}", request.OrderId, userId);
                throw new BadRequestException("The payment could not be verified. If money was deducted, contact the library.");
            }

            var raw = JsonSerializer.Serialize(new { source = "checkout", request.OrderId, request.PaymentId });
            var (outcome, bookingId) = await _bookingRepository.ConfirmPaymentAsync(request.OrderId, request.PaymentId, raw, cancellationToken);
            LogOutcome(outcome, payment.GatewayOrderId, bookingId);

            var booking = await _bookingRepository.GetByIdAsync(bookingId, cancellationToken);
            return new PaymentResultDto
            {
                Outcome = outcome,
                Message = outcome switch
                {
                    PaymentOutcome.SeatLost => "Payment received, but your seat hold had ended and the seat was taken. The library will refund you.",
                    PaymentOutcome.InvoiceClosed => "Payment received, but this fee was already paid or cancelled. The library will refund you.",
                    _ when payment.FeeInvoiceId is not null => "Payment successful. Thank you!",
                    _ => "Payment successful. Your seat is booked!"
                },
                Booking = booking is null ? null : BookingDto.FromEntity(booking, UtcNow)
            };
        }

        public async Task MarkFailedAsync(int userId, PaymentFailedRequest request, CancellationToken cancellationToken = default)
        {
            await GetOwnPaymentAsync(userId, request.OrderId, cancellationToken);

            var reason = string.IsNullOrWhiteSpace(request.Reason) ? "Payment failed or cancelled" : request.Reason.Trim();
            await _paymentRepository.MarkFailedAsync(request.OrderId, request.PaymentId, reason, null, cancellationToken);
            _logger.LogInformation("Payment order {OrderId} failed: {Reason}", request.OrderId, reason);
        }

        public async Task HandleWebhookAsync(string payload, string signature, CancellationToken cancellationToken = default)
        {
            if (!_gateway.VerifyWebhookSignature(payload, signature))
            {
                _logger.LogWarning("Rejected payment webhook with an invalid signature");
                throw new UnauthorizedException("Invalid webhook signature.");
            }

            using var document = JsonDocument.Parse(payload);
            var root = document.RootElement;
            var eventName = root.TryGetProperty("event", out var e) ? e.GetString() : null;

            // payload.payment.entity holds { id, order_id, error_description, ... } for payment events
            if (!root.TryGetProperty("payload", out var body)
                || !body.TryGetProperty("payment", out var paymentElement)
                || !paymentElement.TryGetProperty("entity", out var entity))
            {
                _logger.LogInformation("Ignoring payment webhook {Event} without a payment entity", eventName);
                return;
            }

            var orderId = entity.TryGetProperty("order_id", out var o) ? o.GetString() : null;
            var paymentId = entity.TryGetProperty("id", out var p) ? p.GetString() : null;
            if (string.IsNullOrEmpty(orderId) || await _paymentRepository.GetByOrderIdAsync(orderId, cancellationToken) is null)
            {
                _logger.LogInformation("Ignoring payment webhook {Event} for unknown order {OrderId}", eventName, orderId);
                return;
            }

            switch (eventName)
            {
                case "payment.captured":
                case "order.paid":
                    var (outcome, bookingId) = await _bookingRepository.ConfirmPaymentAsync(orderId, paymentId ?? string.Empty, payload, cancellationToken);
                    LogOutcome(outcome, orderId, bookingId);
                    break;

                case "payment.failed":
                    var reason = entity.TryGetProperty("error_description", out var d) ? d.GetString() : "Payment failed";
                    await _paymentRepository.MarkFailedAsync(orderId, paymentId, reason, payload, cancellationToken);
                    break;

                default:
                    _logger.LogInformation("Ignoring payment webhook {Event}", eventName);
                    break;
            }
        }

        private async Task<Payment> GetOwnPaymentAsync(int userId, string orderId, CancellationToken cancellationToken)
        {
            var payment = await _paymentRepository.GetByOrderIdAsync(orderId, cancellationToken);
            if (payment is null || payment.UserId != userId)
            {
                throw new NotFoundException("Payment order not found.");
            }
            return payment;
        }

        private void LogOutcome(PaymentOutcome outcome, string orderId, int bookingId)
        {
            if (outcome is PaymentOutcome.SeatLost or PaymentOutcome.InvoiceClosed)
            {
                _logger.LogWarning("Order {OrderId} (booking {BookingId}) was paid but could not be applied ({Outcome}). REFUND REQUIRED.", orderId, bookingId, outcome);
            }
            else
            {
                _logger.LogInformation("Order {OrderId} confirmed booking {BookingId} ({Outcome})", orderId, bookingId, outcome);
            }
        }
    }
}
