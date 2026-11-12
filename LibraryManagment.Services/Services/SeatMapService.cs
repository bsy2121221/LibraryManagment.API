using LibraryManagement.DataAccess.IRepositories;
using LibraryManagement.Models.DTOs.SeatMap;
using LibraryManagement.Services.IServices;

namespace LibraryManagement.Services.Services
{
    public class SeatMapService : ISeatMapService
    {
        private readonly ISeatRepository _seatRepository;

        public SeatMapService(ISeatRepository seatRepository)
        {
            _seatRepository = seatRepository;
        }

        public Task<SeatMapDto> GetAsync(int? userId, CancellationToken cancellationToken = default)
        {
            return _seatRepository.GetSeatMapAsync(userId, cancellationToken);
        }
    }
}
