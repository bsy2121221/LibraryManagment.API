using LibraryManagement.Models.Entities;

namespace LibraryManagement.DataAccess.IRepositories
{
    public interface ISectionRepository
    {
        Task<IReadOnlyList<SectionWithStats>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<SectionWithStats?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<int> CreateAsync(Section section, CancellationToken cancellationToken = default);
        Task UpdateAsync(Section section, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
