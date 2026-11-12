using LibraryManagement.Models.Enums;

namespace LibraryManagement.Models.Entities
{
    public class Payment
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? BookingId { get; set; }
        public int? FeeInvoiceId { get; set; }
        public string Gateway { get; set; } = string.Empty;
        public string GatewayOrderId { get; set; } = string.Empty;
        public string? GatewayPaymentId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public PaymentStatus Status { get; set; }
        public string? FailureReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
