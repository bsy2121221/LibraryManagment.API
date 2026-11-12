using LibraryManagement.Models.Entities;

namespace LibraryManagement.Auth.Services
{
    public record IssuedToken(string Token, DateTime ExpiresAt);

    public interface ITokenService
    {
        IssuedToken CreateAccessToken(User user);

        // Returns the raw token for the client; only its hash is stored.
        IssuedToken CreateRefreshToken();

        string HashRefreshToken(string refreshToken);
    }
}
