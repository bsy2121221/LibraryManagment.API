using LibraryManagement.Models.DTOs.SeatMap;
using LibraryManagement.Models.Entities;

namespace LibraryManagement.DataAccess.IRepositories
{
    public interface ISeatRepository
    {
        Task<IReadOnlyList<SeatWithOccupant>> GetBySectionAsync(int sectionId, CancellationToken cancellationToken = default);

        // Replaces the section's layout: seats with Id = 0 are created, missing seats are removed
        Task SaveLayoutAsync(int sectionId, IEnumerable<Seat> seats, CancellationToken cancellationToken = default);

        Task<SeatMapDto> GetSeatMapAsync(int? userId, CancellationToken cancellationToken = default);
    }
}
