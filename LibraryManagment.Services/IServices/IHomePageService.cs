using LibraryManagement.Models.DTOs.HomePage;
using System.Text.Json;

namespace LibraryManagement.Services.IServices
{
    // Home page designed by the admin with blocks. The header and footer are fixed and not part of the layout.
    public interface IHomePageService
    {
        // null = nothing published yet (the website shows its built-in default page)
        Task<PublicHomePageDto?> GetPublishedAsync(CancellationToken cancellationToken = default);

        // Admin builder
        Task<AdminHomePageDto> GetForAdminAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<HomePageVersionDto>> GetHistoryAsync(CancellationToken cancellationToken = default);
        Task<HomePageVersionDto> GetVersionAsync(int id, CancellationToken cancellationToken = default);
        Task<HomePageVersionDto> SaveAsync(int adminUserId, JsonElement layout, bool publish, CancellationToken cancellationToken = default);
        Task<HomePageVersionDto> PublishAsync(int adminUserId, int id, CancellationToken cancellationToken = default);
    }

    public interface IMediaService
    {
        Task<IReadOnlyList<MediaAssetDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<MediaAssetDto> UploadAsync(int adminUserId, Stream content, string fileName, long length, CancellationToken cancellationToken = default);
        Task DeleteAsync(int adminUserId, int id, CancellationToken cancellationToken = default);
    }
}
