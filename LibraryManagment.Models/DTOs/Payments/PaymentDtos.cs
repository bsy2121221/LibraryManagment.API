using LibraryManagement.Models.DTOs.Bookings;
using LibraryManagement.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models.DTOs.Payments
{
    // Everything the browser needs to open the checkout. For Razorpay these map 1:1 to Razorpay Checkout options.
    public class PaymentOrderDto
    {
        // "Mock" or "Razorpay": the UI shows the fake payment screen for Mock
        public string Provider { get; set; } = string.Empty;
        public string KeyId { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;

        // In the smallest unit (paise), as Razorpay expects
        public long AmountInPaise { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";

        public string LibraryName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public PaymentPrefillDto Prefill { get; set; } = new();
        public BookingDto Booking { get; set; } = new();
    }

    public class PaymentPrefillDto
    {
        public string Name { get; set; } = string.Empty;
        public string Contact { get; set; } = string.Empty;
        public string? Email { get; set; }
    }

    // The three values Razorpay Checkout passes to its success handler
    public class VerifyPaymentRequest
    {
        [Required, StringLength(100)]
        public string OrderId { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string PaymentId { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string Signature { get; set; } = string.Empty;
    }

    public class PaymentFailedRequest
    {
        [Required, StringLength(100)]
        public string OrderId { get; set; } = string.Empty;

        [StringLength(100)]
        public string? PaymentId { get; set; }

        [StringLength(500)]
        public string? Reason { get; set; }
    }

    public class PaymentResultDto
    {
        public PaymentOutcome Outcome { get; set; }
        public string Message { get; set; } = string.Empty;
        public BookingDto? Booking { get; set; }
    }
}
