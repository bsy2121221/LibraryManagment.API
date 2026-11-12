using LibraryManagement.Models.Enums;

namespace LibraryManagement.Models.Entities
{
    // Row of dbo.vw_BookingDetails: a booking with its seat, section and student
    public class BookingDetails
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int SeatId { get; set; }
        public BookingStatus Status { get; set; }
        public DateTime? HoldExpiresAt { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? NextDueDate { get; set; }
        public DateTime? ReleasedAt { get; set; }
        public string? ReleaseReason { get; set; }
        public DateTime CreatedAt { get; set; }

        public string SeatLabel { get; set; } = string.Empty;
        public int SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public Gender SectionGender { get; set; }

        public string StudentName { get; set; } = string.Empty;
        public string StudentMobile { get; set; } = string.Empty;
        public string? StudentEmail { get; set; }

        public bool IsHoldActive(DateTime utcNow) => Status == BookingStatus.Held && HoldExpiresAt > utcNow;
    }
}
