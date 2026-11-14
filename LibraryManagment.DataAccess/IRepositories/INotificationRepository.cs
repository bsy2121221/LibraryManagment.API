using LibraryManagement.Models.Entities;
using LibraryManagement.Models.Enums;

namespace LibraryManagement.DataAccess.IRepositories
{
    // dbo.Notifications is a queue: stored procedures add Ready rows, the dispatcher claims and sends them
    public interface INotificationRepository
    {
        // Dispatcher
        Task<IReadOnlyList<QueuedNotification>> ClaimBatchAsync(int batchSize, int lockSeconds, CancellationToken cancellationToken = default);
        Task MarkSentAsync(long id, string provider, string? providerMessageId, string? subject, string body, CancellationToken cancellationToken = default);
        Task MarkFailedAsync(long id, string? provider, string error, int retryDelaySeconds, bool permanent, bool skip, CancellationToken cancellationToken = default);

        // Billing job
        Task<int> EnqueueFeeRemindersAsync(int graceDays, CancellationToken cancellationToken = default);

        // Admin log
        Task<IReadOnlyList<NotificationLogEntry>> GetForAdminAsync(NotificationStatus? status, NotificationChannel? channel,
            string? eventType, int? userId, string? search, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<NotificationStatusCount>> GetSummaryAsync(CancellationToken cancellationToken = default);
        Task RetryAsync(long id, CancellationToken cancellationToken = default);
        Task CancelAsync(long id, CancellationToken cancellationToken = default);

        // Admin settings
        Task<IReadOnlyList<NotificationEventChannelRow>> GetEventChannelsAsync(CancellationToken cancellationToken = default);
        Task SaveEventChannelAsync(string eventType, NotificationChannel channel, bool isActive, string? subject, string body, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<NotificationRule>> GetRulesAsync(CancellationToken cancellationToken = default);
        Task<int> SaveRuleAsync(NotificationRule rule, CancellationToken cancellationToken = default);
        Task DeleteRuleAsync(int id, CancellationToken cancellationToken = default);
    }
}
