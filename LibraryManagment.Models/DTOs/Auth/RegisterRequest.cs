using LibraryManagement.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models.DTOs.Auth
{
    public class RegisterRequest
    {
        [Required, StringLength(150, MinimumLength = 2)]
        public string FullName { get; set; } = string.Empty;

        [Required, RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Enter a valid 10 digit mobile number.")]
        public string Mobile { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;

        [Required, EnumDataType(typeof(Gender))]
        public Gender? Gender { get; set; }

        [Required, StringLength(500, MinimumLength = 5)]
        public string Address { get; set; } = string.Empty;

        [EmailAddress, StringLength(256)]
        public string? Email { get; set; }
    }
}
