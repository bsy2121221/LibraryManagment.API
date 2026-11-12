using LibraryManagement.Models.DTOs.Sections;

namespace LibraryManagement.Services.IServices
{
    // Admin: library sections (Boys / Girls) and their seat layouts
    public interface ISectionService
    {
        Task<IReadOnlyList<SectionDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<SectionLayoutDto> GetLayoutAsync(int id, CancellationToken cancellationToken = default);
        Task<SectionDto> CreateAsync(SectionRequest request, CancellationToken cancellationToken = default);
        Task<SectionDto> UpdateAsync(int id, SectionRequest request, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<SectionLayoutDto> SaveLayoutAsync(int id, SaveSeatLayoutRequest request, CancellationToken cancellationToken = default);
        Task<SectionLayoutDto> GenerateSeatsAsync(int id, GenerateSeatsRequest request, CancellationToken cancellationToken = default);
    }
}
