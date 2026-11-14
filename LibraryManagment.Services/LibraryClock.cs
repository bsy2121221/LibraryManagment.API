namespace LibraryManagement.Services
{
    // The library's local date. The stored procedures use India Standard Time too.
    public static class LibraryClock
    {
        private static readonly TimeZoneInfo LibraryTimeZone = FindTimeZone();

        public static DateOnly Today(TimeProvider timeProvider) =>
            DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), LibraryTimeZone).DateTime);

        private static TimeZoneInfo FindTimeZone()
        {
            // "Asia/Kolkata" works on Linux and on Windows (.NET maps IANA ids); the Windows id is the fallback
            foreach (var id in new[] { "Asia/Kolkata", "India Standard Time" })
            {
                if (TimeZoneInfo.TryFindSystemTimeZoneById(id, out var zone)) return zone;
            }
            return TimeZoneInfo.CreateCustomTimeZone("IST", TimeSpan.FromHours(5.5), "India Standard Time", "India Standard Time");
        }
    }
}
