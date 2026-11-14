using LibraryManagement.Models.Entities;

namespace LibraryManagement.DataAccess.IRepositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<User?> GetByMobileAsync(string mobile, CancellationToken cancellationToken = default);
        Task<bool> MobileExistsAsync(string mobile, CancellationToken cancellationToken = default);
        Task<bool> AnyAdminExistsAsync(CancellationToken cancellationToken = default);
        Task<int> CreateAsync(User user, CancellationToken cancellationToken = default);

        // Admin student management
        Task<IReadOnlyList<StudentSummary>> GetStudentsForAdminAsync(string? search, bool? isActive, bool? hasSeat, CancellationToken cancellationToken = default);
        Task<int> CreateByAdminAsync(User user, CancellationToken cancellationToken = default);
        Task UpdateProfileAsync(User user, CancellationToken cancellationToken = default);
        Task UpdatePasswordAsync(int userId, string passwordHash, CancellationToken cancellationToken = default);
    }
}
