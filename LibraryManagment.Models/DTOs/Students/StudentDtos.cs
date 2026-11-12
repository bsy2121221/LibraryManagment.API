using LibraryManagement.Models.DTOs.Bookings;
using LibraryManagement.Models.Entities;
using LibraryManagement.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models.DTOs.Students
{
    public class StudentListItemDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string? Email { get; set; }
        public Gender? Gender { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // Current seat; null when the student has none
        public int? BookingId { get; set; }
        public BookingStatus? BookingStatus { get; set; }
        public string? SeatLabel { get; set; }
        public string? SectionName { get; set; }
        public DateOnly? NextDueDate { get; set; }

        public static StudentListItemDto FromEntity(StudentSummary s) => new()
        {
            Id = s.Id,
            FullName = s.FullName,
            Mobile = s.Mobile,
            Email = s.Email,
            Gender = s.Gender,
            IsActive = s.IsActive,
            CreatedAt = s.CreatedAt,
            BookingId = s.BookingId,
            BookingStatus = s.BookingStatus,
            SeatLabel = s.SeatLabel,
            SectionName = s.SectionName,
            NextDueDate = s.NextDueDate
        };
    }

    public class StudentProfileDto
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

        public static StudentProfileDto FromEntity(User u) => new()
        {
            Id = u.Id,
            FullName = u.FullName,
            Mobile = u.Mobile,
            Email = u.Email,
            Gender = u.Gender,
            Address = u.Address,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt,
            UpdatedAt = u.UpdatedAt
        };
    }

    // Everything on the admin's student page
    public class StudentDetailDto
    {
        public StudentProfileDto Profile { get; set; } = new();
        public BookingDto? CurrentBooking { get; set; }
        public List<SeatChangeDto> SeatHistory { get; set; } = [];
        public List<StudentPaymentDto> Payments { get; set; } = [];
    }

    public class SeatChangeDto
    {
        public DateTime ChangedAt { get; set; }
        public string? FromSeat { get; set; }
        public string? ToSeat { get; set; }
        public string? Reason { get; set; }
        public string ChangedBy { get; set; } = string.Empty;

        public static SeatChangeDto FromEntity(SeatChangeEntry e) => new()
        {
            ChangedAt = e.ChangedAt,
            FromSeat = e.FromSeatLabel is null ? null : $"{e.FromSeatLabel} ({e.FromSectionName})",
            ToSeat = e.ToSeatLabel is null ? null : $"{e.ToSeatLabel} ({e.ToSectionName})",
            Reason = e.Reason,
            ChangedBy = e.ChangedByName
        };
    }

    public class StudentPaymentDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public PaymentStatus Status { get; set; }
        public string Gateway { get; set; } = string.Empty;
        public string GatewayOrderId { get; set; } = string.Empty;
        public string? GatewayPaymentId { get; set; }
        public string? FailureReason { get; set; }

        public static StudentPaymentDto FromEntity(Payment p) => new()
        {
            Id = p.Id,
            CreatedAt = p.CreatedAt,
            Amount = p.Amount,
            Currency = p.Currency,
            Status = p.Status,
            Gateway = p.Gateway,
            GatewayOrderId = p.GatewayOrderId,
            GatewayPaymentId = p.GatewayPaymentId,
            FailureReason = p.FailureReason
        };
    }

    // Same rules as registration, plus IsActive
    public class UpdateStudentRequest
    {
        [Required, StringLength(150, MinimumLength = 2)]
        public string FullName { get; set; } = string.Empty;

        [Required, RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Enter a valid 10 digit mobile number.")]
        public string Mobile { get; set; } = string.Empty;

        [EmailAddress, StringLength(256)]
        public string? Email { get; set; }

        [Required, EnumDataType(typeof(Gender))]
        public Gender? Gender { get; set; }

        [Required, StringLength(500, MinimumLength = 5)]
        public string Address { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }

    public class ResetPasswordRequest
    {
        [Required, StringLength(100, MinimumLength = 8)]
        public string NewPassword { get; set; } = string.Empty;
    }
}
