using LibraryManagement.Models.DTOs.Auth;

namespace LibraryManagement.Services.IServices
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
        Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
        Task<AuthResponse> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);
        Task LogoutAsync(int userId, string refreshToken, CancellationToken cancellationToken = default);
        Task<UserProfileDto> GetProfileAsync(int userId, CancellationToken cancellationToken = default);
    }
}
