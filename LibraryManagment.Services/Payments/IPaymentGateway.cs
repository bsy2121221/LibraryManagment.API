namespace LibraryManagement.Services.Payments
{
    // One implementation per payment provider. The active one is chosen by "Payment:Provider" in appsettings.json.
    public interface IPaymentGateway
    {
        // Stored in dbo.Payments.Gateway and sent to the UI ("Mock" / "Razorpay")
        string Name { get; }

        // Public key the browser checkout needs (Razorpay KeyId). Never the secret.
        string PublicKey { get; }

        Task<GatewayOrder> CreateOrderAsync(long amountInPaise, string currency, string receipt,
            IDictionary<string, string> notes, CancellationToken cancellationToken = default);

        // Checks the signature the checkout returns after a successful payment
        bool VerifyPaymentSignature(string orderId, string paymentId, string signature);

        // Checks the signature header of a server-to-server webhook call
        bool VerifyWebhookSignature(string payload, string signature);
    }

    public record GatewayOrder(string OrderId, long AmountInPaise, string Currency);
}
