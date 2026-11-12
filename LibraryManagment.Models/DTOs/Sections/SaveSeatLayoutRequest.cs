using LibraryManagement.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models.DTOs.Sections
{
    // The complete layout of a section. Seats with Id = null are created, seats with an Id are updated,
    // and existing seats left out of the list are removed.
    public class SaveSeatLayoutRequest
    {
        [Required, MaxLength(1000)]
        public List<SeatLayoutItem> Seats { get; set; } = [];
    }

    public class SeatLayoutItem
    {
        public int? Id { get; set; }

        [Required, StringLength(20, MinimumLength = 1)]
        public string SeatLabel { get; set; } = string.Empty;

        [Range(0, 5000)]
        public decimal PosX { get; set; }

        [Range(0, 5000)]
        public decimal PosY { get; set; }

        [Range(-360, 360)]
        public short Rotation { get; set; }

        [EnumDataType(typeof(SeatStatus))]
        public SeatStatus Status { get; set; } = SeatStatus.Active;
    }
}
