namespace LibraryManagement.Models.Entities
{
    // Admin notice shown to everyone in the rotating bar above the navbar (dbo.SiteNotices)
    public class SiteNotice
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
