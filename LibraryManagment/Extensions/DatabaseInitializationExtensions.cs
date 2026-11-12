using LibraryManagement.Services.IServices;

namespace LibraryManagement.API.Extensions
{
    public static class DatabaseInitializationExtensions
    {
        // Schema and stored procedures are deployed manually from LibraryManagement.Database.
        // The first admin is seeded here because the password must be BCrypt-hashed in code.
        public static async Task SeedAdminAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            await scope.ServiceProvider.GetRequiredService<IAdminSeeder>().SeedAsync();
        }
    }
}
