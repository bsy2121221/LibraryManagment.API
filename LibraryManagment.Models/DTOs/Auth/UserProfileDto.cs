using LibraryManagement.Models.Entities;
using LibraryManagement.Models.Enums;

namespace LibraryManagement.Models.DTOs.Auth
{
    public class UserProfileDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string? Email { get; set; }
        public Gender? Gender { get; set; }
        public string? Address { get; set; }
        public UserRole Role { get; set; }

        public static UserProfileDto FromEntity(User user) => new()
        {
            Id = user.Id,
            FullName = user.FullName,
            Mobile = user.Mobile,
            Email = user.Email,
            Gender = user.Gender,
            Address = user.Address,
            Role = user.Role
        };
    }
}
