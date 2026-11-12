using LibraryManagement.Models.Entities;

namespace LibraryManagement.DataAccess.IRepositories
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default);
        Task CreateAsync(RefreshToken token, CancellationToken cancellationToken = default);
        Task RevokeAsync(int id, string? replacedByTokenHash, CancellationToken cancellationToken = default);
        Task RevokeAllForUserAsync(int userId, CancellationToken cancellationToken = default);
    }
}
