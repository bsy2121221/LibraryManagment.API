using LibraryManagement.Models.Entities;

namespace LibraryManagement.DataAccess.IRepositories
{
    public interface IPaymentRepository
    {
        Task<int> CreateAsync(Payment payment, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Payment>> GetByUserAsync(int userId, CancellationToken cancellationToken = default);
        Task<Payment?> GetByOrderIdAsync(string gatewayOrderId, CancellationToken cancellationToken = default);
        Task MarkFailedAsync(string gatewayOrderId, string? gatewayPaymentId, string? reason, string? rawResponse, CancellationToken cancellationToken = default);
    }
}
