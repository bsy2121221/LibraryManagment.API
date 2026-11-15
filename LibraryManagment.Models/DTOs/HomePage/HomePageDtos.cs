using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace LibraryManagement.Models.DTOs.HomePage
{
    // The live home page for visitors. Layout is the JSON designed in the admin builder:
    // { "blocks": [ { "id": "...", "type": "hero", ... }, ... ] }
    public class PublicHomePageDto
    {
        public int Version { get; set; }
        public JsonElement Layout { get; set; }
        public DateTime? PublishedAt { get; set; }
    }

    // One saved version (Layout is omitted in the history list)
    public class HomePageVersionDto
    {
        public int Id { get; set; }
        public int Version { get; set; }

        // "Draft" | "Published"
        public string Status { get; set; } = string.Empty;

        // True for the version visitors currently see
        public bool IsLive { get; set; }
        public JsonElement? Layout { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
    }

    // What the builder opens: the latest saved version (null if nothing saved yet) and the live version number
    public class AdminHomePageDto
    {
        public HomePageVersionDto? Latest { get; set; }
        public int? LiveVersion { get; set; }
    }

    public class SaveHomePageRequest
    {
        [Required]
        public JsonElement Layout { get; set; }

        // true = save and make it the live home page
        public bool Publish { get; set; }
    }

    public class MediaAssetDto
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long SizeBytes { get; set; }
        public string UploadedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
