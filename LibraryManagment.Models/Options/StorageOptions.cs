namespace LibraryManagement.Models.Options
{
    public class StorageOptions
    {
        public const string SectionName = "Storage";

        // "Local" | "AzureBlob"
        public string Provider { get; set; } = "Local";
        public LocalStorageOptions Local { get; set; } = new();
        public AzureBlobStorageOptions AzureBlob { get; set; } = new();
    }

    public class LocalStorageOptions
    {
        public string RootPath { get; set; } = "wwwroot/uploads";
    }

    public class AzureBlobStorageOptions
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string Container { get; set; } = "media";
    }
}
