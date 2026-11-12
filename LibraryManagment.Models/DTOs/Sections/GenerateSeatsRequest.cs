using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models.DTOs.Sections
{
    // Adds Rows x Columns seats in a grid, so the admin can start from "40 seats" and then drag them into place.
    // Labels are LabelPrefix + number (e.g. B1, B2...), skipping labels already used in the section.
    public class GenerateSeatsRequest
    {
        [Range(1, 50)]
        public int Rows { get; set; }

        [Range(1, 50)]
        public int Columns { get; set; }

        [StringLength(10)]
        public string? LabelPrefix { get; set; }

        [Range(1, 9999)]
        public int StartNumber { get; set; } = 1;

        [Range(0, 5000)]
        public decimal OriginX { get; set; } = 40;

        [Range(0, 5000)]
        public decimal OriginY { get; set; } = 40;

        [Range(10, 1000)]
        public decimal SpacingX { get; set; } = 60;

        [Range(10, 1000)]
        public decimal SpacingY { get; set; } = 60;
    }
}
