using LibraryManagement.Models.Entities;
using LibraryManagement.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models.DTOs.Fees
{
    public class FeeInvoiceDto
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentMobile { get; set; } = string.Empty;
        public string SeatLabel { get; set; } = string.Empty;
        public string SectionName { get; set; } = string.Empty;
        public DateOnly PeriodStart { get; set; }
        public DateOnly PeriodEnd { get; set; }
        public decimal Amount { get; set; }
        public DateOnly DueDate { get; set; }
        public InvoiceStatus Status { get; set; }
        public DateTime? PaidAt { get; set; }

        // "Razorpay", "Mock", "Cash"... for paid fees
        public string? PaidVia { get; set; }

        // Due or Overdue: can be paid now
        public bool IsPayable => Status is InvoiceStatus.Due or InvoiceStatus.Overdue;

        public static FeeInvoiceDto FromEntity(FeeInvoiceDetails i) => new()
        {
            Id = i.Id,
            BookingId = i.BookingId,
            StudentId = i.UserId,
            StudentName = i.StudentName,
            StudentMobile = i.StudentMobile,
            SeatLabel = i.SeatLabel,
            SectionName = i.SectionName,
            PeriodStart = i.PeriodStart,
            PeriodEnd = i.PeriodEnd,
            Amount = i.Amount,
            DueDate = i.DueDate,
            Status = i.Status,
            PaidAt = i.PaidAt,
            PaidVia = i.PaidVia
        };
    }

    // Important popup shown to a student after login while they have an overdue fee
    public class OverdueNoticeDto
    {
        public string Title { get; set; } = string.Empty;

        // Admin text with {Placeholders} already filled in (plain text, line breaks kept)
        public string Message { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }

        // The overdue fees, oldest first, each payable with the normal fee payment-order flow
        public IReadOnlyList<FeeInvoiceDto> Fees { get; set; } = [];
    }

    public class MarkFeePaidRequest
    {
        [StringLength(500)]
        public string? Note { get; set; }
    }
}
