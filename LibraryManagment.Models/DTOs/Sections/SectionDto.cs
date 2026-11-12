using LibraryManagement.Models.Entities;
using LibraryManagement.Models.Enums;

namespace LibraryManagement.Models.DTOs.Sections
{
    public class SectionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Gender Gender { get; set; }
        public int LayoutWidth { get; set; }
        public int LayoutHeight { get; set; }
        public string? BackgroundImageUrl { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public int TotalSeats { get; set; }
        public int DisabledSeats { get; set; }
        public int OccupiedSeats { get; set; }
        public int AvailableSeats => TotalSeats - DisabledSeats - OccupiedSeats;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public static SectionDto FromEntity(SectionWithStats s) => new()
        {
            Id = s.Id,
            Name = s.Name,
            Gender = s.Gender,
            LayoutWidth = s.LayoutWidth,
            LayoutHeight = s.LayoutHeight,
            BackgroundImageUrl = s.BackgroundImageUrl,
            DisplayOrder = s.DisplayOrder,
            IsActive = s.IsActive,
            TotalSeats = s.TotalSeats,
            DisabledSeats = s.DisabledSeats,
            OccupiedSeats = s.OccupiedSeats,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt
        };
    }
}
