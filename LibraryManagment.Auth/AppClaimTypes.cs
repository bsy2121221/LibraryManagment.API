namespace LibraryManagement.Auth
{
    // Short JWT claim names. Inbound claim mapping is disabled, so these are what controllers see.
    public static class AppClaimTypes
    {
        public const string UserId = "sub";
        public const string Name = "name";
        public const string Role = "role";
        public const string Mobile = "mobile";
        public const string Gender = "gender";
    }
}
