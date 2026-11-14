using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models.DTOs.Fees
{
    // Admin adds a fee for a student who has a seat
    public class CreateFeeRequest
    {
        [Range(1, int.MaxValue)]
        public int StudentId { get; set; }

        [Required]
        public DateOnly? PeriodStart { get; set; }

        // Defaults to one month after PeriodStart, minus a day
        public DateOnly? PeriodEnd { get; set; }

        // Defaults to the MonthlyFee setting
        [Range(typeof(decimal), "1", "100000")]
        public decimal? Amount { get; set; }

        // Defaults to PeriodStart
        public DateOnly? DueDate { get; set; }

        public bool PaidAtDesk { get; set; }

        [StringLength(500)]
        public string? Note { get; set; }
    }
}
