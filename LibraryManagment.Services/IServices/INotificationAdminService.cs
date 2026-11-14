using LibraryManagement.Models.DTOs.Notifications;
using LibraryManagement.Models.Enums;

namespace LibraryManagement.Services.IServices
{
    // Admin: message log, which channels each event uses, and fee reminder rules
    public interface INotificationAdminService
    {
        Task<IReadOnlyList<NotificationDto>> GetMessagesAsync(NotificationStatus? status, NotificationChannel? channel, string? eventType,
            int? studentId, string? search, CancellationToken cancellationToken = default);
        Task<NotificationSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
        Task RetryAsync(long id, CancellationToken cancellationToken = default);
        Task CancelAsync(long id, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<NotificationEventDto>> GetEventsAsync(CancellationToken cancellationToken = default);
        Task<NotificationEventDto> SaveEventAsync(string eventType, SaveNotificationEventRequest request, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<NotificationRuleDto>> GetRulesAsync(CancellationToken cancellationToken = default);
        Task<NotificationRuleDto> SaveRuleAsync(int? id, SaveNotificationRuleRequest request, CancellationToken cancellationToken = default);
        Task DeleteRuleAsync(int id, CancellationToken cancellationToken = default);
    }
}
