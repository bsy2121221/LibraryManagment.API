using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models.DTOs.Settings
{
    public class AdminSettingDto
    {
        public string Key { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;

        // "string" | "int" | "decimal" | "bool"
        public string DataType { get; set; } = "string";
        public string? Description { get; set; }

        // Groups settings on the admin screen, e.g. "General", "Fees & booking"
        public string Group { get; set; } = "Other";
        public decimal? Min { get; set; }
        public decimal? Max { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    // Only the settings being changed need to be sent: { "values": { "MonthlyFee": "1200" } }
    public class UpdateSettingsRequest
    {
        [Required, MinLength(1)]
        public Dictionary<string, string> Values { get; set; } = [];
    }
}
