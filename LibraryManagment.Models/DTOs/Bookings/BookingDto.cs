using LibraryManagement.Models.Entities;
using LibraryManagement.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models.DTOs.Bookings
{
    public class BookingDto
    {
        public int Id { get; set; }
        public BookingStatus Status { get; set; }
        public int SeatId { get; set; }
        public string SeatLabel { get; set; } = string.Empty;
        public int SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public Gender SectionGender { get; set; }

        // While Held: when the hold ends, and seconds left measured on the server (so a wrong PC clock does not matter)
        public DateTime? HoldExpiresAt { get; set; }
        public int? HoldSecondsRemaining { get; set; }

        public DateOnly? StartDate { get; set; }
        // Earliest unpaid fee's due date (or the next period's start when everything is paid)
        public DateOnly? NextDueDate { get; set; }
        public bool HasOverdueFee { get; set; }
        public DateTime? ReleasedAt { get; set; }
        public string? ReleaseReason { get; set; }
        public DateTime CreatedAt { get; set; }

        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentMobile { get; set; } = string.Empty;

        public static BookingDto FromEntity(BookingDetails b, DateTime utcNow) => new()
        {
            Id = b.Id,
            Status = b.Status,
            SeatId = b.SeatId,
            SeatLabel = b.SeatLabel,
            SectionId = b.SectionId,
            SectionName = b.SectionName,
            SectionGender = b.SectionGender,
            HoldExpiresAt = b.Status == BookingStatus.Held ? b.HoldExpiresAt : null,
            HoldSecondsRemaining = b.Status == BookingStatus.Held && b.HoldExpiresAt is { } expires
                ? Math.Max(0, (int)(expires - utcNow).TotalSeconds)
                : null,
            StartDate = b.StartDate,
            NextDueDate = b.NextDueDate,
            HasOverdueFee = b.HasOverdueFee,
            ReleasedAt = b.ReleasedAt,
            ReleaseReason = b.ReleaseReason,
            CreatedAt = b.CreatedAt,
            StudentId = b.UserId,
            StudentName = b.StudentName,
            StudentMobile = b.StudentMobile
        };
    }

    public class HoldSeatRequest
    {
        [Range(1, int.MaxValue)]
        public int SeatId { get; set; }
    }

    public class ReleaseBookingRequest
    {
        [Required, StringLength(500, MinimumLength = 3)]
        public string Reason { get; set; } = string.Empty;
    }

    public class MoveSeatRequest
    {
        [Range(1, int.MaxValue)]
        public int ToSeatId { get; set; }

        [StringLength(500)]
        public string? Reason { get; set; }
    }
}
