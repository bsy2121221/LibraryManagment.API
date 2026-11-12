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

        // Admin
        Task<IReadOnlyList<AdminSettingDto>> GetAllForAdminAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<AdminSettingDto>> UpdateAsync(int adminUserId, IDictionary<string, string> values, CancellationToken cancellationToken = default);
    }
}
