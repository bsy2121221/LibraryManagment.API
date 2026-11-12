using LibraryManagement.Auth.Services;
using LibraryManagement.DataAccess.IRepositories;
using LibraryManagement.Models.DTOs.Auth;
using LibraryManagement.Models.Entities;
using LibraryManagement.Models.Enums;
using LibraryManagement.Models.Exceptions;
using LibraryManagement.Services.IServices;
using Microsoft.Extensions.Logging;

namespace LibraryManagement.Services.Services
{
    public class AuthService : IAuthService
    {
        private const string InvalidCredentialsMessage = "Invalid mobile number or password.";
        private const string InvalidRefreshTokenMessage = "Session expired. Please log in again.";

        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IPasswordHasher passwordHasher,
            ITokenService tokenService,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
        {
            var mobile = request.Mobile.Trim();
            if (await _userRepository.MobileExistsAsync(mobile, cancellationToken))
            {
                throw new ConflictException("This mobile number is already registered.");
            }

            var user = new User
            {
                FullName = request.FullName.Trim(),
                Mobile = mobile,
                Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
                PasswordHash = _passwordHasher.Hash(request.Password),
                Gender = request.Gender,
                Address = request.Address.Trim(),
                Role = UserRole.Student,
                IsMobileVerified = false,
                IsActive = true
            };
            user.Id = await _userRepository.CreateAsync(user, cancellationToken);

            _logger.LogInformation("Student {UserId} registered", user.Id);
            return await IssueTokensAsync(user, cancellationToken);
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByMobileAsync(request.Mobile.Trim(), cancellationToken);
            if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedException(InvalidCredentialsMessage);
            }

            if (!user.IsActive)
            {
                throw new ForbiddenException("Your account has been deactivated. Please contact the library.");
            }

            return await IssueTokensAsync(user, cancellationToken);
        }

        public async Task<AuthResponse> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            var tokenHash = _tokenService.HashRefreshToken(refreshToken);
            var stored = await _refreshTokenRepository.GetByHashAsync(tokenHash, cancellationToken)
                ?? throw new UnauthorizedException(InvalidRefreshTokenMessage);

            if (stored.RevokedAt is not null)
            {
                // A rotated-out token was used again: assume it leaked and end every session for this user.
                _logger.LogWarning("Revoked refresh token reused for user {UserId}; revoking all sessions", stored.UserId);
                await _refreshTokenRepository.RevokeAllForUserAsync(stored.UserId, cancellationToken);
                throw new UnauthorizedException(InvalidRefreshTokenMessage);
            }

            if (stored.ExpiresAt <= DateTime.UtcNow)
            {
                throw new UnauthorizedException(InvalidRefreshTokenMessage);
            }

            var user = await _userRepository.GetByIdAsync(stored.UserId, cancellationToken);
            if (user is null || !user.IsActive)
            {
                await _refreshTokenRepository.RevokeAsync(stored.Id, null, cancellationToken);
                throw new UnauthorizedException(InvalidRefreshTokenMessage);
            }

            var response = await IssueTokensAsync(user, cancellationToken);
            await _refreshTokenRepository.RevokeAsync(
                stored.Id, _tokenService.HashRefreshToken(response.RefreshToken), cancellationToken);
            return response;
        }

        public async Task LogoutAsync(int userId, string refreshToken, CancellationToken cancellationToken = default)
        {
            var stored = await _refreshTokenRepository.GetByHashAsync(
                _tokenService.HashRefreshToken(refreshToken), cancellationToken);

            if (stored is not null && stored.UserId == userId)
            {
                await _refreshTokenRepository.RevokeAsync(stored.Id, null, cancellationToken);
            }
        }

        public async Task<UserProfileDto> GetProfileAsync(int userId, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
                ?? throw new NotFoundException("User not found.");
            return UserProfileDto.FromEntity(user);
        }

        private async Task<AuthResponse> IssueTokensAsync(User user, CancellationToken cancellationToken)
        {
            var accessToken = _tokenService.CreateAccessToken(user);
            var refreshToken = _tokenService.CreateRefreshToken();

            await _refreshTokenRepository.CreateAsync(new RefreshToken
            {
                UserId = user.Id,
                TokenHash = _tokenService.HashRefreshToken(refreshToken.Token),
                ExpiresAt = refreshToken.ExpiresAt
            }, cancellationToken);

            return new AuthResponse
            {
                AccessToken = accessToken.Token,
                AccessTokenExpiresAt = accessToken.ExpiresAt,
                RefreshToken = refreshToken.Token,
                RefreshTokenExpiresAt = refreshToken.ExpiresAt,
                User = UserProfileDto.FromEntity(user)
            };
        }
    }
}
