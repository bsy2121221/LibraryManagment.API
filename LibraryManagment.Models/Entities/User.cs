using LibraryManagement.Models.Enums;

namespace LibraryManagement.Models.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
        public Gender? Gender { get; set; }
        public string? Address { get; set; }
        public UserRole Role { get; set; }
        public bool IsMobileVerified { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
