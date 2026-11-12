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
    }
}
