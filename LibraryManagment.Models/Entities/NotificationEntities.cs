using LibraryManagement.Models.Enums;

namespace LibraryManagement.Models.Entities
{
    // A message claimed by the dispatcher (usp_Notification_ClaimBatch)
    public class QueuedNotification
    {
        public long Id { get; set; }
        public int UserId { get; set; }
        public string EventType { get; set; } = string.Empty;
        public NotificationChannel Channel { get; set; }
        public string Recipient { get; set; } = string.Empty;
        public string? Subject { get; set; }
        public string Body { get; set; } = string.Empty;
        public string? DataJson { get; set; }
        public int Attempts { get; set; }
        public int MaxAttempts { get; set; }
    }

    // Row of the admin message log (usp_Notification_GetForAdmin)
    public class NotificationLogEntry
    {
        public long Id { get; set; }
        public int UserId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public NotificationChannel Channel { get; set; }
        public string Recipient { get; set; } = string.Empty;
        public string? Subject { get; set; }
        public string Body { get; set; } = string.Empty;
        public string? DataJson { get; set; }
        public NotificationStatus Status { get; set; }
        public int Attempts { get; set; }
        public int MaxAttempts { get; set; }
        public DateTime NextAttemptAt { get; set; }
        public string? Provider { get; set; }
        public string? LastError { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? SentAt { get; set; }
        public string? RuleName { get; set; }
    }

    public class NotificationStatusCount
    {
        public NotificationStatus Status { get; set; }
        public int Count { get; set; }
    }

    public class NotificationEventChannelRow
    {
        public string EventType { get; set; } = string.Empty;
        public NotificationChannel Channel { get; set; }
        public bool IsActive { get; set; }
        public int TemplateId { get; set; }
        public string? Subject { get; set; }
        public string Body { get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }
    }

    public class NotificationRule
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public NotificationTrigger Trigger { get; set; }
        public int OffsetDays { get; set; }
        public int? RepeatEveryMinutes { get; set; }
        public int MaxAttempts { get; set; }
        public TimeSpan? QuietHoursStart { get; set; }
        public TimeSpan? QuietHoursEnd { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<NotificationRuleChannelRow> Channels { get; set; } = [];
    }

    public class NotificationRuleChannelRow
    {
        public int RuleId { get; set; }
        public NotificationChannel Channel { get; set; }
        public string? Subject { get; set; }
        public string Body { get; set; } = string.Empty;
    }

    // Row of dbo.vw_FeeInvoiceDetails
    public class FeeInvoiceDetails
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int BookingId { get; set; }
        public DateOnly PeriodStart { get; set; }
        public DateOnly PeriodEnd { get; set; }
        public decimal Amount { get; set; }
        public DateOnly DueDate { get; set; }
        public InvoiceStatus Status { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string SeatLabel { get; set; } = string.Empty;
        public string SectionName { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string StudentMobile { get; set; } = string.Empty;
        public string? PaidVia { get; set; }
    }

    public class BillingRunResult
    {
        public int InvoicesCreated { get; set; }
        public int InvoicesMarkedOverdue { get; set; }
        public int SeatsReleased { get; set; }
    }
}
