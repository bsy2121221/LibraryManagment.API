using LibraryManagement.Models.Entities;

namespace LibraryManagement.DataAccess.IRepositories
{
    public interface ISiteNoticeRepository
    {
        Task<IReadOnlyList<SiteNotice>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SiteNotice>> GetCurrentAsync(DateOnly today, CancellationToken cancellationToken = default);

        // id null = create
        Task<SiteNotice> SaveAsync(int? id, string message, DateOnly? startDate, DateOnly? endDate, bool isActive, int sortOrder, int adminUserId, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
