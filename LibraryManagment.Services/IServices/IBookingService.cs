using LibraryManagement.Models.DTOs.Bookings;
using LibraryManagement.Models.DTOs.Students;
using LibraryManagement.Models.Enums;

namespace LibraryManagement.Services.IServices
{
    public interface IBookingService
    {
        // Student
        Task<BookingDto> HoldSeatAsync(int userId, int seatId, CancellationToken cancellationToken = default);
        Task<BookingDto?> GetMyBookingAsync(int userId, CancellationToken cancellationToken = default);
        Task CancelHoldAsync(int userId, int bookingId, CancellationToken cancellationToken = default);

        // Admin
        Task<IReadOnlyList<BookingDto>> GetAllAsync(BookingStatus? status, string? search, CancellationToken cancellationToken = default);
        Task<BookingDto> AssignSeatAsync(int adminUserId, int studentId, AssignSeatRequest request, CancellationToken cancellationToken = default);
        Task ReleaseAsync(int adminUserId, int bookingId, string reason, CancellationToken cancellationToken = default);
        Task<BookingDto> MoveSeatAsync(int adminUserId, int bookingId, MoveSeatRequest request, CancellationToken cancellationToken = default);
    }
}
