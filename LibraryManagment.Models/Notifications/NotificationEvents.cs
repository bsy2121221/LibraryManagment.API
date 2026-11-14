using LibraryManagement.Models.Enums;

namespace LibraryManagement.Models.Notifications
{
    // Events that queue messages to a student. The EventType strings are written by the stored procedures
    // (usp_Notification_EnqueueEvent calls) and must match exactly.
    public sealed record NotificationEventInfo(string EventType, string Name, string Description, IReadOnlyList<string> Placeholders);

    public static class NotificationEvents
    {
        public const string SeatBooked = "SeatBooked";
        public const string SeatMoved = "SeatMoved";
        public const string SeatReleased = "SeatReleased";
        public const string FeePaid = "FeePaid";
        public const string ProfileUpdated = "ProfileUpdated";
        public const string AccountDeactivated = "AccountDeactivated";
        public const string AccountReactivated = "AccountReactivated";
        public const string PasswordReset = "PasswordReset";
        public const string AccountCreated = "AccountCreated";
        public const string SeatAssigned = "SeatAssigned";
        public const string FeeCreated = "FeeCreated";

        // Queued by the reminder rules, not configurable here
        public const string FeeReminder = "FeeReminder";

        // Available in every message
        public static readonly IReadOnlyList<string> CommonPlaceholders = ["Name", "LibraryName"];

        public static readonly IReadOnlyList<NotificationEventInfo> All =
        [
            new(AccountCreated, "Account created by admin", "When the admin registers a walk-in student (the password is never sent).", ["Mobile"]),
            new(SeatBooked, "Seat booked", "After a student pays for a new seat.", ["Seat", "Section", "Amount", "StartDate", "NextDueDate"]),
            new(SeatAssigned, "Seat assigned by admin", "When the admin gives a seat to a student at the desk.", ["Seat", "Section", "Amount", "StartDate", "NextDueDate", "FeeNote"]),
            new(FeeCreated, "Fee added by admin", "When the admin adds a fee that is still to be paid.", ["Amount", "PeriodStart", "PeriodEnd", "DueDate", "NextDueDate", "Seat", "Section"]),
            new(FeePaid, "Monthly fee paid", "After a monthly fee is paid online or recorded by the admin.", ["Amount", "PeriodStart", "PeriodEnd", "NextDueDate", "Seat", "Section"]),
            new(SeatMoved, "Seat changed by admin", "When the admin moves a student to another seat.", ["FromSeat", "ToSeat", "Section", "Reason"]),
            new(SeatReleased, "Seat released", "When the admin (or the overdue auto-release) removes a student from their seat.", ["Seat", "Section", "Reason"]),
            new(ProfileUpdated, "Details changed by admin", "When the admin edits a student's name, mobile, email, gender or address.", ["Changes"]),
            new(AccountDeactivated, "Account deactivated", "When the admin deactivates a student's account.", []),
            new(AccountReactivated, "Account reactivated", "When the admin activates a deactivated account again.", []),
            new(PasswordReset, "Password reset by admin", "When the admin sets a new password (the password itself is never sent).", []),
        ];

        public static NotificationEventInfo? Find(string eventType) =>
            All.FirstOrDefault(e => string.Equals(e.EventType, eventType, StringComparison.OrdinalIgnoreCase));

        // Placeholders a fee reminder rule's messages can use
        public static readonly IReadOnlyList<string> ReminderPlaceholders =
            ["Amount", "DueDate", "PeriodStart", "PeriodEnd", "DaysOverdue", "Seat", "Section"];

        public static readonly IReadOnlyList<NotificationChannel> Channels =
            [NotificationChannel.Sms, NotificationChannel.WhatsApp, NotificationChannel.Email, NotificationChannel.Call];
    }
}
