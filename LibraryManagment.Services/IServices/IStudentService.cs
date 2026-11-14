using LibraryManagement.Models.DTOs.Students;

namespace LibraryManagement.Services.IServices
{
    // Admin: view and edit student accounts
    public interface IStudentService
    {
        Task<IReadOnlyList<StudentListItemDto>> GetAllAsync(string? search, bool? isActive, bool? hasSeat, CancellationToken cancellationToken = default);
        Task<StudentDetailDto> CreateAsync(int adminUserId, CreateStudentRequest request, CancellationToken cancellationToken = default);
        Task<StudentDetailDto> GetDetailAsync(int studentId, CancellationToken cancellationToken = default);
        Task<StudentDetailDto> UpdateAsync(int adminUserId, int studentId, UpdateStudentRequest request, CancellationToken cancellationToken = default);
        Task ResetPasswordAsync(int adminUserId, int studentId, string newPassword, CancellationToken cancellationToken = default);
    }
}
