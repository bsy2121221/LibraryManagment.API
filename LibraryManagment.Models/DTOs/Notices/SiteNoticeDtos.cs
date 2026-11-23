using LibraryManagement.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models.DTOs.Notices
{
    // Public: what the notice bar shows
    public class PublicNoticeDto
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    // Admin list
    public class SiteNoticeDto
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Active and today is inside the date range, i.e. visible on the website right now
        public bool IsShowingNow { get; set; }

        public static SiteNoticeDto FromEntity(SiteNotice n, DateOnly today) => new()
        {
            Id = n.Id,
            Message = n.Message,
            StartDate = n.StartDate,
            EndDate = n.EndDate,
            IsActive = n.IsActive,
            SortOrder = n.SortOrder,
            CreatedAt = n.CreatedAt,
            UpdatedAt = n.UpdatedAt,
            IsShowingNow = n.IsActive && (n.StartDate is null || n.StartDate <= today) && (n.EndDate is null || n.EndDate >= today)
        };
    }

    // Used for both create and update
    public class SiteNoticeRequest
    {
        [Required, StringLength(300, MinimumLength = 3)]
        public string Message { get; set; } = string.Empty;

        // Show from / until (inclusive). Empty = no limit.
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        [Range(0, 1000)]
        public int SortOrder { get; set; }
    }
}
