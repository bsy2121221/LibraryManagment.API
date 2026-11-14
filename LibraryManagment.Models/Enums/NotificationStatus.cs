namespace LibraryManagement.Models.Enums
{
    // Lifecycle of a row in dbo.Notifications (the message queue)
    public enum NotificationStatus : byte
    {
        Ready = 1,       // waiting for the background job
        Processing = 2,  // being sent right now
        Sent = 3,
        Failed = 4,      // gave up after MaxAttempts (or a permanent error)
        Skipped = 5,     // nothing to send with (e.g. no sender for the channel)
        Cancelled = 6    // stopped by the admin before it was sent
    }
}
