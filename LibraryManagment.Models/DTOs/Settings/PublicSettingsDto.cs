namespace LibraryManagement.Models.DTOs.Settings
{
    // Settings any visitor may see (shown on the seat map and checkout)
    public class PublicSettingsDto
    {
        public string LibraryName { get; set; } = string.Empty;
        public decimal MonthlyFee { get; set; }
        public int SeatHoldMinutes { get; set; }
        public string Currency { get; set; } = "INR";

        // Shown in the website footer. Empty values are hidden.
        public FooterDto Footer { get; set; } = new();
    }

    public class FooterDto
    {
        public string About { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string WhatsAppNumber { get; set; } = string.Empty;
        public string OpeningHours { get; set; } = string.Empty;
        public string GoogleMapsUrl { get; set; } = string.Empty;
        public string InstagramUrl { get; set; } = string.Empty;
        public string FacebookUrl { get; set; } = string.Empty;
        public string YouTubeUrl { get; set; } = string.Empty;
    }
}
