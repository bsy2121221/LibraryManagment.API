using LibraryManagement.Models.Enums;
using Microsoft.Extensions.Logging;

namespace LibraryManagement.Services.Notifications
{
    // Used while a channel's Provider is "Mock": nothing leaves the server, the message is written to the log
    // (console + logs/log-*.txt) and marked Sent with Provider = "Mock", so the whole flow can be tested.
    public class MockNotificationSender : INotificationSender
    {
        private readonly ILogger<MockNotificationSender> _logger;

        public MockNotificationSender(NotificationChannel channel, ILogger<MockNotificationSender> logger)
        {
            Channel = channel;
            _logger = logger;
        }

        public NotificationChannel Channel { get; }

        public string ProviderName => "Mock";

        public Task<string?> SendAsync(OutgoingMessage message, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[MOCK {Channel}] #{NotificationId} to {Recipient}{Subject}: {Body}",
                message.Channel, message.NotificationId, message.Recipient,
                message.Subject is null ? string.Empty : $" | {message.Subject}", message.Body);

            return Task.FromResult<string?>($"mock_{Guid.NewGuid():N}"[..20]);
        }
    }
}
