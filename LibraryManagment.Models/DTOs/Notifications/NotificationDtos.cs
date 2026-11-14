using LibraryManagement.Models.Entities;
using LibraryManagement.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models.DTOs.Notifications
{
    // ---------- Message log / queue ----------

    public class NotificationDto
    {
        public long Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
        public NotificationChannel Channel { get; set; }
        public string Recipient { get; set; } = string.Empty;
        public string? Subject { get; set; }

        // Final text once sent; template text with {Placeholders} while still waiting
        public string Body { get; set; } = string.Empty;
        public NotificationStatus Status { get; set; }
        public int Attempts { get; set; }
        public int MaxAttempts { get; set; }
        public DateTime NextAttemptAt { get; set; }
        public string? Provider { get; set; }
        public string? LastError { get; set; }
        public string? RuleName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? SentAt { get; set; }

        public static NotificationDto FromEntity(NotificationLogEntry n, string eventName) => new()
        {
            Id = n.Id,
            StudentId = n.UserId,
            StudentName = n.StudentName,
            EventType = n.EventType,
            EventName = eventName,
            Channel = n.Channel,
            Recipient = n.Recipient,
            Subject = n.Subject,
            Body = n.Body,
            Status = n.Status,
            Attempts = n.Attempts,
            MaxAttempts = n.MaxAttempts,
            NextAttemptAt = n.NextAttemptAt,
            Provider = n.Provider,
            LastError = n.LastError,
            RuleName = n.RuleName,
            CreatedAt = n.CreatedAt,
            SentAt = n.SentAt
        };
    }

    public class NotificationSummaryDto
    {
        public int Ready { get; set; }
        public int Processing { get; set; }
        public int Sent { get; set; }
        public int Failed { get; set; }
        public int Skipped { get; set; }
        public int Cancelled { get; set; }

        // Which provider each channel uses right now ("Mock", "Smtp", ...), from appsettings.json
        public Dictionary<NotificationChannel, string> Providers { get; set; } = [];
    }

    // ---------- Event messages ----------

    public class NotificationEventDto
    {
        public string EventType { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Placeholders { get; set; } = [];

        // Always all four channels; IsActive = false for channels that are off or not set up
        public List<EventChannelDto> Channels { get; set; } = [];
    }

    public class EventChannelDto
    {
        public NotificationChannel Channel { get; set; }
        public bool IsActive { get; set; }
        public string? Subject { get; set; }
        public string Body { get; set; } = string.Empty;
    }

    public class SaveNotificationEventRequest
    {
        [Required, MinLength(1), MaxLength(4)]
        public List<EventChannelRequest> Channels { get; set; } = [];
    }

    public class EventChannelRequest
    {
        [EnumDataType(typeof(NotificationChannel))]
        public NotificationChannel Channel { get; set; }

        public bool IsActive { get; set; }

        [StringLength(200)]
        public string? Subject { get; set; }

        [StringLength(4000)]
        public string? Body { get; set; }
    }

    // ---------- Fee reminder rules ----------

    public class NotificationRuleDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public NotificationTrigger Trigger { get; set; }
        public int OffsetDays { get; set; }
        public int? RepeatEveryMinutes { get; set; }
        public int MaxAttempts { get; set; }

        // "HH:mm" in India time, or null
        public string? QuietHoursStart { get; set; }
        public string? QuietHoursEnd { get; set; }
        public bool IsActive { get; set; }
        public List<RuleChannelDto> Channels { get; set; } = [];

        public static NotificationRuleDto FromEntity(NotificationRule r) => new()
        {
            Id = r.Id,
            Name = r.Name,
            Trigger = r.Trigger,
            OffsetDays = r.OffsetDays,
            RepeatEveryMinutes = r.RepeatEveryMinutes,
            MaxAttempts = r.MaxAttempts,
            QuietHoursStart = r.QuietHoursStart?.ToString(@"hh\:mm"),
            QuietHoursEnd = r.QuietHoursEnd?.ToString(@"hh\:mm"),
            IsActive = r.IsActive,
            Channels = r.Channels.Select(c => new RuleChannelDto { Channel = c.Channel, Subject = c.Subject, Body = c.Body }).ToList()
        };
    }

    public class RuleChannelDto
    {
        [EnumDataType(typeof(NotificationChannel))]
        public NotificationChannel Channel { get; set; }

        [StringLength(200)]
        public string? Subject { get; set; }

        [Required, StringLength(4000, MinimumLength = 5)]
        public string Body { get; set; } = string.Empty;
    }

    public class SaveNotificationRuleRequest
    {
        [Required, StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [EnumDataType(typeof(NotificationTrigger))]
        public NotificationTrigger Trigger { get; set; }

        [Range(0, 60)]
        public int OffsetDays { get; set; }

        // null = send once; 60 = every hour; 1440 = every day
        [Range(15, 43200)]
        public int? RepeatEveryMinutes { get; set; }

        // Most reminders per channel when repeating
        [Range(1, 50)]
        public int MaxAttempts { get; set; } = 1;

        [RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "Use HH:mm, e.g. 21:00.")]
        public string? QuietHoursStart { get; set; }

        [RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "Use HH:mm, e.g. 09:00.")]
        public string? QuietHoursEnd { get; set; }

        public bool IsActive { get; set; } = true;

        [Required, MinLength(1), MaxLength(4)]
        public List<RuleChannelDto> Channels { get; set; } = [];
    }
}
