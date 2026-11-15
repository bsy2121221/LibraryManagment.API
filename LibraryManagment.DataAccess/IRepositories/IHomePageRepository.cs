using LibraryManagement.Models.Entities;

namespace LibraryManagement.DataAccess.IRepositories
{
    // Home page designs (dbo.HomePageLayouts) and the media library (dbo.MediaAssets)
    public interface IHomePageRepository
    {
        Task<HomePageLayout?> GetPublishedAsync(CancellationToken cancellationToken = default);

        // id null = latest saved version
        Task<HomePageLayout?> GetByIdAsync(int? id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<HomePageLayout>> GetHistoryAsync(int top, CancellationToken cancellationToken = default);
        Task<HomePageLayout> SaveAsync(string layoutJson, bool publish, int createdByUserId, CancellationToken cancellationToken = default);
        Task PublishAsync(int id, CancellationToken cancellationToken = default);
    }

    public interface IMediaRepository
    {
        Task<IReadOnlyList<MediaAsset>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<MediaAsset> CreateAsync(MediaAsset asset, CancellationToken cancellationToken = default);

        // Returns the stored file name so the file can be removed
        Task<string> DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
