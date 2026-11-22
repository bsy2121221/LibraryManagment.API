using LibraryManagement.Services.IServices;

namespace LibraryManagement.API.Extensions
{
    public static class DatabaseInitializationExtensions
    {
        // Schema and stored procedures are deployed manually from LibraryManagement.Database.
        // The first admin is seeded here because the password must be BCrypt-hashed in code.
        public static async Task SeedAdminAsync(this WebApplication app)
        {
            try
            {
                using var scope = app.Services.CreateScope();
                await scope.ServiceProvider.GetRequiredService<IAdminSeeder>().SeedAsync();
            }
            catch (Exception ex)
            {
                // Do not prevent the host from starting when the DB is unreachable or credentials are wrong.
                app.Logger.LogError(ex, "Admin seed failed. Check ConnectionStrings:DefaultConnection and that the schema is deployed.");
            }
        }
    }
}
