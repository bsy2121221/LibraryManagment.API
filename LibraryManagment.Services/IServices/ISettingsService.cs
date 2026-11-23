using LibraryManagement.Models.DTOs.Settings;

namespace LibraryManagement.Services.IServices
{
    // Business settings stored in dbo.Settings, edited by the admin on the Settings screen
    public interface ISettingsService
    {
        Task<PublicSettingsDto> GetPublicAsync(CancellationToken cancellationToken = default);
        Task<string> GetLibraryNameAsync(CancellationToken cancellationToken = default);
        Task<decimal> GetMonthlyFeeAsync(CancellationToken cancellationToken = default);
        Task<int> GetSeatHoldMinutesAsync(CancellationToken cancellationToken = default);
        Task<BillingSettings> GetBillingSettingsAsync(CancellationToken cancellationToken = default);

        // Admin-written popup for students with an overdue fee; Message is empty when the popup is switched off
        Task<(string LibraryName, string Title, string Message)> GetOverdueNoticeAsync(CancellationToken cancellationToken = default);

        // Admin
        Task<IReadOnlyList<AdminSettingDto>> GetAllForAdminAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<AdminSettingDto>> UpdateAsync(int adminUserId, IDictionary<string, string> values, CancellationToken cancellationToken = default);
    }

    // Values the billing job uses (see SettingDefinitions for their meaning and limits)
    public sealed record BillingSettings(decimal MonthlyFee, int InvoiceDaysBefore, int GraceDays, int AutoReleaseAfterOverdueDays);
}
