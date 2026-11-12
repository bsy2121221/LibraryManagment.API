using LibraryManagement.Models.Enums;

namespace LibraryManagement.Models.Entities
{
    public class Section
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Gender Gender { get; set; }
        public int LayoutWidth { get; set; }
        public int LayoutHeight { get; set; }
        public string? BackgroundImageUrl { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    // Row returned by usp_Section_GetAll / usp_Section_GetById
    public class SectionWithStats : Section
    {
        public int TotalSeats { get; set; }
        public int DisabledSeats { get; set; }
        public int OccupiedSeats { get; set; }
    }
}
