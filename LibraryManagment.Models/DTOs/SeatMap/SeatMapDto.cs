using LibraryManagement.Models.Enums;

namespace LibraryManagement.Models.DTOs.SeatMap
{
    // Public seat map: green = Available, red = Occupied, grey/red = Disabled. No student details are exposed.
    public class SeatMapDto
    {
        public List<SeatMapSectionDto> Sections { get; set; } = [];
    }

    public class SeatMapSectionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Gender Gender { get; set; }
        public int LayoutWidth { get; set; }
        public int LayoutHeight { get; set; }
        public string? BackgroundImageUrl { get; set; }
        public int DisplayOrder { get; set; }
        public int TotalSeats => Seats.Count;
        public int AvailableSeats => Seats.Count(s => s.Availability == SeatAvailability.Available);
        public List<SeatMapSeatDto> Seats { get; set; } = [];
    }

    public class SeatMapSeatDto
    {
        public int Id { get; set; }
        public int SectionId { get; set; }
        public string SeatLabel { get; set; } = string.Empty;
        public decimal PosX { get; set; }
        public decimal PosY { get; set; }
        public short Rotation { get; set; }
        public SeatAvailability Availability { get; set; }

        // True when the logged-in caller holds or has booked this seat
        public bool IsMine { get; set; }
    }
}
