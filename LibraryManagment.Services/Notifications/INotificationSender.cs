using LibraryManagement.Models.Enums;

namespace LibraryManagement.Services.Notifications
{
    // Sends one message on one channel. One implementation per provider; which one is used for a channel
    // is chosen by "Notifications:<Channel>:Provider" in appsettings.json (see NotificationSenderRegistration).
    public interface INotificationSender
    {
        NotificationChannel Channel { get; }

        // Stored in dbo.Notifications.Provider ("Mock", "Smtp", "Msg91", "Meta", "Twilio")
        string ProviderName { get; }

        // Returns the provider's message id. Throws NotificationSendException (or any exception) on failure.
        Task<string?> SendAsync(OutgoingMessage message, CancellationToken cancellationToken = default);
    }

    public sealed record OutgoingMessage(long NotificationId, NotificationChannel Channel, string Recipient, string? Subject, string Body);

    // Permanent = retrying will not help (bad number, rejected by provider); the message is marked Failed at once
    public class NotificationSendException(string message, bool permanent = false, Exception? inner = null)
        : Exception(message, inner)
    {
        public bool Permanent { get; } = permanent;
    }
}
