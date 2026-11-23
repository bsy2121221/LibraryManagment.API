namespace LibraryManagement.Models.Entities
{
    public class Setting
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string DataType { get; set; } = "string";
        public string? Description { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedByUserId { get; set; }
    }

    // Keys seeded by Script.PostDeployment.sql
    public static class SettingKeys
    {
        public const string LibraryName = "LibraryName";
        public const string MonthlyFee = "MonthlyFee";
        public const string SeatHoldMinutes = "SeatHoldMinutes";
        public const string InvoiceGenerationDaysBefore = "InvoiceGenerationDaysBefore";
        public const string GraceDays = "GraceDays";
        public const string AutoReleaseAfterOverdueDays = "AutoReleaseAfterOverdueDays";

        // Website footer (fixed design, only these values are editable)
        public const string FooterAbout = "FooterAbout";
        public const string LibraryAddress = "LibraryAddress";
        public const string ContactPhone = "ContactPhone";
        public const string ContactEmail = "ContactEmail";
        public const string WhatsAppNumber = "WhatsAppNumber";
        public const string OpeningHours = "OpeningHours";
        public const string GoogleMapsUrl = "GoogleMapsUrl";
        public const string InstagramUrl = "InstagramUrl";
        public const string FacebookUrl = "FacebookUrl";
        public const string YouTubeUrl = "YouTubeUrl";

        // Important popup shown to a student with an overdue fee after login (empty message = no popup)
        public const string OverdueNoticeTitle = "OverdueNoticeTitle";
        public const string OverdueNoticeMessage = "OverdueNoticeMessage";
    }
}
