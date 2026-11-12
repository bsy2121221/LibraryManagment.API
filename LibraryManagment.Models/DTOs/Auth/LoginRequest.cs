using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models.DTOs.Auth
{
    public class LoginRequest
    {
        [Required]
        public string Mobile { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
