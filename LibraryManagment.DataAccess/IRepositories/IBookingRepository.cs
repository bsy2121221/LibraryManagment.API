using LibraryManagement.Models.Entities;
using LibraryManagement.Models.Enums;

namespace LibraryManagement.DataAccess.IRepositories
{
    public interface IBookingRepository
    {
        Task<BookingDetails?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<BookingDetails?> GetLiveByUserAsync(int userId, CancellationToken cancellationToken = default);
        Task<BookingDetails> HoldAsync(int userId, int seatId, int holdMinutes, CancellationToken cancellationToken = default);
        Task CancelHoldAsync(int bookingId, int userId, CancellationToken cancellationToken = default);
        Task<int> ExpireHoldsAsync(CancellationToken cancellationToken = default);

        // Activates the booking paid by this gateway order. Safe to call more than once.
        Task<(PaymentOutcome Outcome, int BookingId)> ConfirmPaymentAsync(
            string gatewayOrderId, string gatewayPaymentId, string? rawResponse, CancellationToken cancellationToken = default);

        // status null = live bookings (Active + Held)
        Task<IReadOnlyList<BookingDetails>> GetAllForAdminAsync(BookingStatus? status, string? search, CancellationToken cancellationToken = default);
        Task ReleaseAsync(int bookingId, int adminUserId, string reason, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SeatChangeEntry>> GetSeatHistoryByUserAsync(int userId, CancellationToken cancellationToken = default);
        Task MoveSeatAsync(int bookingId, int toSeatId, int adminUserId, string? reason, CancellationToken cancellationToken = default);
    }
}
