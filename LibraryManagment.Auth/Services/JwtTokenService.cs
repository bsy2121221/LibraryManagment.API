using LibraryManagement.Models.Entities;
using LibraryManagement.Models.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Buffers.Text;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace LibraryManagement.Auth.Services
{
    public class JwtTokenService : ITokenService
    {
        private readonly JwtOptions _options;
        private readonly SigningCredentials _signingCredentials;
        private readonly JsonWebTokenHandler _tokenHandler = new();

        public JwtTokenService(IOptions<JwtOptions> options)
        {
            _options = options.Value;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
            _signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        }

        public IssuedToken CreateAccessToken(User user)
        {
            var expiresAt = DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes);

            var claims = new List<Claim>
            {
                new(AppClaimTypes.UserId, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(AppClaimTypes.Name, user.FullName),
                new(AppClaimTypes.Mobile, user.Mobile),
                new(AppClaimTypes.Role, user.Role.ToString())
            };
            if (user.Gender is { } gender)
            {
                claims.Add(new Claim(AppClaimTypes.Gender, gender.ToString()));
            }

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = _options.Issuer,
                Audience = _options.Audience,
                Expires = expiresAt,
                SigningCredentials = _signingCredentials
            };

            return new IssuedToken(_tokenHandler.CreateToken(descriptor), expiresAt);
        }

        public IssuedToken CreateRefreshToken()
        {
            var token = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(64));
            return new IssuedToken(token, DateTime.UtcNow.AddDays(_options.RefreshTokenDays));
        }

        public string HashRefreshToken(string refreshToken) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
    }
}
