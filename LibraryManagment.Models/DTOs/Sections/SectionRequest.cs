using LibraryManagement.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models.DTOs.Sections
{
    // Used for both create and update
    public class SectionRequest
    {
        [Required, StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required, EnumDataType(typeof(Gender))]
        public Gender? Gender { get; set; }

        [Range(200, 5000)]
        public int LayoutWidth { get; set; } = 1000;

        [Range(200, 5000)]
        public int LayoutHeight { get; set; } = 600;

        [StringLength(500)]
        public string? BackgroundImageUrl { get; set; }

        [Range(0, 1000)]
        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
