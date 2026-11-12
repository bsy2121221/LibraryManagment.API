using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models.DTOs.Auth
{
    public class RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
