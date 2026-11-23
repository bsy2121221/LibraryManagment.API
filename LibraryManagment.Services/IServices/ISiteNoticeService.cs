using LibraryManagement.Models.DTOs.Notices;

namespace LibraryManagement.Services.IServices
{
    public interface ISiteNoticeService
    {
        // Public: notices showing today, in display order
        Task<IReadOnlyList<PublicNoticeDto>> GetCurrentAsync(CancellationToken cancellationToken = default);

        // Admin
        Task<IReadOnlyList<SiteNoticeDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<SiteNoticeDto> SaveAsync(int adminUserId, int? id, SiteNoticeRequest request, CancellationToken cancellationToken = default);
        Task DeleteAsync(int adminUserId, int id, CancellationToken cancellationToken = default);
    }
}
