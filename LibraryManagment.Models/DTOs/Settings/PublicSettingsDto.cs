namespace LibraryManagement.Models.DTOs.Settings
{
    // Settings any visitor may see (shown on the seat map and checkout)
    public class PublicSettingsDto
    {
        public string LibraryName { get; set; } = string.Empty;
        public decimal MonthlyFee { get; set; }
        public int SeatHoldMinutes { get; set; }
        public string Currency { get; set; } = "INR";
    }
}
