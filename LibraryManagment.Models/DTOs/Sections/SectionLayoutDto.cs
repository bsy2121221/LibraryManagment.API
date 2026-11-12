using LibraryManagement.Models.Entities;
using LibraryManagement.Models.Enums;

namespace LibraryManagement.Models.DTOs.Sections
{
    // Admin designer view: the section plus every seat, with who is sitting on it
    public class SectionLayoutDto
    {
        public SectionDto Section { get; set; } = new();
        public List<AdminSeatDto> Seats { get; set; } = [];
    }

    public class AdminSeatDto
    {
        public int Id { get; set; }
        public string SeatLabel { get; set; } = string.Empty;
        public decimal PosX { get; set; }
        public decimal PosY { get; set; }
        public short Rotation { get; set; }
        public SeatStatus Status { get; set; }
        public SeatOccupantDto? Occupant { get; set; }

        public static AdminSeatDto FromEntity(SeatWithOccupant s) => new()
        {
            Id = s.Id,
            SeatLabel = s.SeatLabel,
            PosX = s.PosX,
            PosY = s.PosY,
            Rotation = s.Rotation,
            Status = s.Status,
            Occupant = s.BookingId is null ? null : new SeatOccupantDto
            {
                BookingId = s.BookingId.Value,
                BookingStatus = s.BookingStatus!.Value,
                HoldExpiresAt = s.HoldExpiresAt,
                StartDate = s.StartDate,
                NextDueDate = s.NextDueDate,
                StudentId = s.StudentId!.Value,
                StudentName = s.StudentName ?? string.Empty,
                StudentMobile = s.StudentMobile ?? string.Empty
            }
        };
    }

    public class SeatOccupantDto
    {
        public int BookingId { get; set; }
        public BookingStatus BookingStatus { get; set; }
        public DateTime? HoldExpiresAt { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? NextDueDate { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentMobile { get; set; } = string.Empty;
    }
}
