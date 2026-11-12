using LibraryManagement.Models.Enums;

namespace LibraryManagement.Models.Entities
{
    // PosX/PosY are the seat's top-left corner in the section's layout coordinates (0..LayoutWidth, 0..LayoutHeight).
    public class Seat
    {
        public int Id { get; set; }
        public int SectionId { get; set; }
        public string SeatLabel { get; set; } = string.Empty;
        public decimal PosX { get; set; }
        public decimal PosY { get; set; }
        public short Rotation { get; set; }
        public SeatStatus Status { get; set; }
    }

    // Row returned by usp_Seat_GetBySection: the seat plus its live booking, if any
    public class SeatWithOccupant : Seat
    {
        public int? BookingId { get; set; }
        public BookingStatus? BookingStatus { get; set; }
        public DateTime? HoldExpiresAt { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? NextDueDate { get; set; }
        public int? StudentId { get; set; }
        public string? StudentName { get; set; }
        public string? StudentMobile { get; set; }
    }
}
