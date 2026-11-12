namespace LibraryManagement.Models.Options
{
    // Used only when no admin exists yet. Set real values via user-secrets / environment variables.
    public class AdminSeedOptions
    {
        public const string SectionName = "AdminSeed";

        public string FullName { get; set; } = "Administrator";
        public string Mobile { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
