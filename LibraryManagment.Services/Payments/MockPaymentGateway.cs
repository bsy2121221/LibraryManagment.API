namespace LibraryManagement.Services.Payments
{
    // For development and testing without a Razorpay account. No money moves.
    // The UI shows a fake payment screen; "Pay" sends Signature = MockPaymentGateway.SuccessSignature.
    public class MockPaymentGateway : IPaymentGateway
    {
        public const string SuccessSignature = "mock_signature_success";

        public string Name => "Mock";

        public string PublicKey => "mock_key";

        public Task<GatewayOrder> CreateOrderAsync(long amountInPaise, string currency, string receipt,
            IDictionary<string, string> notes, CancellationToken cancellationToken = default)
        {
            var orderId = $"order_mock_{Guid.NewGuid():N}"[..30];
            return Task.FromResult(new GatewayOrder(orderId, amountInPaise, currency));
        }

        public bool VerifyPaymentSignature(string orderId, string paymentId, string signature)
            => orderId.StartsWith("order_mock_", StringComparison.Ordinal) && signature == SuccessSignature;

        public bool VerifyWebhookSignature(string payload, string signature) => signature == SuccessSignature;
    }
}
