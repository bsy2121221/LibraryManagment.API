using LibraryManagement.Models.Enums;

namespace LibraryManagement.Models.Entities
{
    // Row of usp_User_GetStudentsForAdmin: a student and their current seat, if any
    public class StudentSummary
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string? Email { get; set; }
        public Gender? Gender { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public int? BookingId { get; set; }
        public BookingStatus? BookingStatus { get; set; }
        public string? SeatLabel { get; set; }
        public string? SectionName { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? NextDueDate { get; set; }
    }

    // Row of usp_SeatChangeHistory_GetByUser
    public class SeatChangeEntry
    {
        public int Id { get; set; }
        public DateTime ChangedAt { get; set; }
        public string? Reason { get; set; }
        public string? FromSeatLabel { get; set; }
        public string? FromSectionName { get; set; }
        public string? ToSeatLabel { get; set; }
        public string? ToSectionName { get; set; }
        public string ChangedByName { get; set; } = string.Empty;
    }
}
