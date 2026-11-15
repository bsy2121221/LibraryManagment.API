namespace LibraryManagement.Models.Entities
{
    // dbo.MediaAssets row (an uploaded image)
    public class MediaAsset
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string StoredFileName { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long SizeBytes { get; set; }
        public int UploadedByUserId { get; set; }
        public string UploadedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    // dbo.HomePageLayouts.Status
    public enum HomePageLayoutStatus : byte
    {
        Draft = 1,
        Published = 2
    }

    // dbo.HomePageLayouts row. LayoutJson is null in the history list.
    public class HomePageLayout
    {
        public int Id { get; set; }
        public int Version { get; set; }
        public string? LayoutJson { get; set; }
        public HomePageLayoutStatus Status { get; set; }
        public int CreatedByUserId { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public bool IsLive { get; set; }
    }
}
