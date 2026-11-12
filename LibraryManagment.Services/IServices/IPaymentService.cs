using LibraryManagement.Models.DTOs.Payments;

namespace LibraryManagement.Services.IServices
{
    public interface IPaymentService
    {
        // Creates a gateway order for the first month's fee of a held seat
        Task<PaymentOrderDto> CreateBookingOrderAsync(int userId, int bookingId, CancellationToken cancellationToken = default);

        // Called by the browser after the checkout reports success
        Task<PaymentResultDto> VerifyAsync(int userId, VerifyPaymentRequest request, CancellationToken cancellationToken = default);

        // Called by the browser when the checkout reports failure or is closed
        Task MarkFailedAsync(int userId, PaymentFailedRequest request, CancellationToken cancellationToken = default);

        // Razorpay server-to-server notification; confirms payments even if the browser was closed
        Task HandleWebhookAsync(string payload, string signature, CancellationToken cancellationToken = default);
    }
}
