using LibraryManagement.Models.DTOs.Fees;
using LibraryManagement.Models.Enums;

namespace LibraryManagement.Services.IServices
{
    public interface IFeeService
    {
        // Student
        Task<IReadOnlyList<FeeInvoiceDto>> GetMyFeesAsync(int userId, CancellationToken cancellationToken = default);

        // null when the student has no overdue fee or the admin left the popup message empty
        Task<OverdueNoticeDto?> GetMyOverdueNoticeAsync(int userId, CancellationToken cancellationToken = default);

        // Admin
        Task<IReadOnlyList<FeeInvoiceDto>> GetAllAsync(InvoiceStatus? status, string? search, CancellationToken cancellationToken = default);
        Task<FeeInvoiceDto> CreateAsync(int adminUserId, CreateFeeRequest request, CancellationToken cancellationToken = default);
        Task<FeeInvoiceDto> MarkPaidAsync(int adminUserId, int invoiceId, string? note, CancellationToken cancellationToken = default);
    }
}
