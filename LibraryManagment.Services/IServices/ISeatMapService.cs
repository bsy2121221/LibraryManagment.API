using LibraryManagement.Models.DTOs.SeatMap;

namespace LibraryManagement.Services.IServices
{
    public interface ISeatMapService
    {
        Task<SeatMapDto> GetAsync(int? userId, CancellationToken cancellationToken = default);
    }
}
