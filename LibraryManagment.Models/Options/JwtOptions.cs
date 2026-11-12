using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models.Options
{
    public class JwtOptions
    {
        public const string SectionName = "Jwt";

        [Required]
        public string Issuer { get; set; } = string.Empty;

        [Required]
        public string Audience { get; set; } = string.Empty;

        // HMAC-SHA256 needs at least 256 bits of key material.
        [Required, MinLength(32, ErrorMessage = "Jwt:SecretKey must be at least 32 characters.")]
        public string SecretKey { get; set; } = string.Empty;

        [Range(1, 1440)]
        public int AccessTokenMinutes { get; set; } = 15;

        [Range(1, 365)]
        public int RefreshTokenDays { get; set; } = 7;
    }
}
