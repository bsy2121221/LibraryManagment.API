using LibraryManagement.Models.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace LibraryManagement.Services.Payments
{
    // Razorpay Orders API + signature checks. https://razorpay.com/docs/payments/server-integration/
    // Turn it on with "Payment": { "Provider": "Razorpay", "Razorpay": { KeyId, KeySecret, WebhookSecret } }.
    // UPI is offered by Razorpay Checkout automatically when it is enabled on the Razorpay account.
    public class RazorpayPaymentGateway : IPaymentGateway
    {
        public const string BaseUrl = "https://api.razorpay.com/v1/";

        private readonly HttpClient _httpClient;
        private readonly RazorpayOptions _options;
        private readonly ILogger<RazorpayPaymentGateway> _logger;

        public RazorpayPaymentGateway(HttpClient httpClient, IOptions<PaymentOptions> options, ILogger<RazorpayPaymentGateway> logger)
        {
            _options = options.Value.Razorpay;
            _logger = logger;
            _httpClient = httpClient;
            _httpClient.BaseAddress ??= new Uri(BaseUrl);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_options.KeyId}:{_options.KeySecret}")));
        }

        public string Name => "Razorpay";

        public string PublicKey => _options.KeyId;

        public async Task<GatewayOrder> CreateOrderAsync(long amountInPaise, string currency, string receipt,
            IDictionary<string, string> notes, CancellationToken cancellationToken = default)
        {
            var request = new RazorpayOrderRequest(amountInPaise, currency, receipt, notes);
            using var response = await _httpClient.PostAsJsonAsync("orders", request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Razorpay order creation failed with {StatusCode}: {Body}", (int)response.StatusCode, body);
                throw new InvalidOperationException($"Razorpay order creation failed ({(int)response.StatusCode}).");
            }

            var order = await response.Content.ReadFromJsonAsync<RazorpayOrderResponse>(cancellationToken)
                ?? throw new InvalidOperationException("Razorpay returned an empty order.");
            return new GatewayOrder(order.Id, order.Amount, order.Currency);
        }

        // signature = HMAC_SHA256(order_id + "|" + payment_id, key_secret) as hex
        public bool VerifyPaymentSignature(string orderId, string paymentId, string signature)
            => SignatureMatches($"{orderId}|{paymentId}", _options.KeySecret, signature);

        // X-Razorpay-Signature = HMAC_SHA256(raw request body, webhook_secret) as hex
        public bool VerifyWebhookSignature(string payload, string signature)
            => !string.IsNullOrEmpty(_options.WebhookSecret) && SignatureMatches(payload, _options.WebhookSecret, signature);

        private static bool SignatureMatches(string payload, string secret, string signature)
        {
            if (string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(secret))
            {
                return false;
            }

            var expected = HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(payload));
            byte[] actual;
            try
            {
                actual = Convert.FromHexString(signature);
            }
            catch (FormatException)
            {
                return false;
            }
            return CryptographicOperations.FixedTimeEquals(expected, actual);
        }

        private sealed record RazorpayOrderRequest(
            [property: JsonPropertyName("amount")] long Amount,
            [property: JsonPropertyName("currency")] string Currency,
            [property: JsonPropertyName("receipt")] string Receipt,
            [property: JsonPropertyName("notes")] IDictionary<string, string> Notes);

        private sealed record RazorpayOrderResponse(
            [property: JsonPropertyName("id")] string Id,
            [property: JsonPropertyName("amount")] long Amount,
            [property: JsonPropertyName("currency")] string Currency);
    }
}
