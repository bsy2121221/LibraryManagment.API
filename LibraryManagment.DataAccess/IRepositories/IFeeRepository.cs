using LibraryManagement.Models.Entities;
using LibraryManagement.Models.Enums;

namespace LibraryManagement.DataAccess.IRepositories
{
    public interface IFeeRepository
    {
        Task<BillingRunResult> RunBillingAsync(decimal monthlyFee, int invoiceDaysBefore, int graceDays, int autoReleaseAfterOverdueDays,
            CancellationToken cancellationToken = default);

        Task<FeeInvoiceDetails?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<FeeInvoiceDetails>> GetByUserAsync(int userId, CancellationToken cancellationToken = default);

        // status null = unpaid (Due + Overdue)
        Task<IReadOnlyList<FeeInvoiceDetails>> GetForAdminAsync(InvoiceStatus? status, string? search, CancellationToken cancellationToken = default);
        // Returns the new invoice id
        Task<int> CreateByAdminAsync(int bookingId, DateOnly periodStart, DateOnly periodEnd, decimal amount, DateOnly dueDate,
            bool paidAtDesk, string? note, int adminUserId, string currency, CancellationToken cancellationToken = default);
        Task MarkPaidAsync(int invoiceId, int adminUserId, string? note, string currency, CancellationToken cancellationToken = default);
    }
}
