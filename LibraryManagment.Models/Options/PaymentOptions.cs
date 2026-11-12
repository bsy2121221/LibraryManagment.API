namespace LibraryManagement.Models.Options
{
    public class PaymentOptions
    {
        public const string SectionName = "Payment";

        // "Mock" | "Razorpay"
        public string Provider { get; set; } = "Mock";
        public string Currency { get; set; } = "INR";
        public RazorpayOptions Razorpay { get; set; } = new();
    }

    public class RazorpayOptions
    {
        public string KeyId { get; set; } = string.Empty;
        public string KeySecret { get; set; } = string.Empty;
        public string WebhookSecret { get; set; } = string.Empty;
    }
}
