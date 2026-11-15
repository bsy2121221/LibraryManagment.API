namespace LibraryManagement.Models.Options
{
    public class StorageOptions
    {
        public const string SectionName = "Storage";

        // "Local" | "AzureBlob" (AzureBlob is not implemented yet)
        public string Provider { get; set; } = "Local";

        // Largest image the admin may upload
        public int MaxUploadMb { get; set; } = 5;
        public LocalStorageOptions Local { get; set; } = new();
        public AzureBlobStorageOptions AzureBlob { get; set; } = new();
    }

    public class LocalStorageOptions
    {
        // Folder for uploaded files, relative to the API's content root (or an absolute path)
        public string RootPath { get; set; } = "wwwroot/uploads";

        // URL path the files are served from. The React dev server proxies /uploads to the API.
        public string RequestPath { get; set; } = "/uploads";
    }

    public class AzureBlobStorageOptions
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string Container { get; set; } = "media";
    }
}
