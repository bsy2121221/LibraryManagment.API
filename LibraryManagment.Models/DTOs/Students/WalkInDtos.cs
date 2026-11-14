using LibraryManagement.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models.DTOs.Students
{
    // Admin registers a walk-in student. Same rules as self-registration.
    public class CreateStudentRequest
    {
        [Required, StringLength(150, MinimumLength = 2)]
        public string FullName { get; set; } = string.Empty;

        [Required, RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Enter a valid 10 digit mobile number.")]
        public string Mobile { get; set; } = string.Empty;

        // The admin tells this password to the student; it is never sent in a message
        [Required, StringLength(100, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;

        [Required, EnumDataType(typeof(Gender))]
        public Gender? Gender { get; set; }

        [Required, StringLength(500, MinimumLength = 5)]
        public string Address { get; set; } = string.Empty;

        [EmailAddress, StringLength(256)]
        public string? Email { get; set; }
    }

    // Admin gives a seat to a student directly, without the online hold and payment
    public class AssignSeatRequest
    {
        [Range(1, int.MaxValue)]
        public int SeatId { get; set; }

        // Defaults to today (library time)
        public DateOnly? StartDate { get; set; }

        // First month's fee; defaults to the MonthlyFee setting
        [Range(typeof(decimal), "0", "100000")]
        public decimal? Amount { get; set; }

        // true = the first month was paid at the desk now; false = it is due on the start date
        public bool PaidAtDesk { get; set; } = true;

        [StringLength(500)]
        public string? Note { get; set; }
    }
}
