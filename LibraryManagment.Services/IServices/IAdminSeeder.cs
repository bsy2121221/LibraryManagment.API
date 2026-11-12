namespace LibraryManagement.Services.IServices
{
    public interface IAdminSeeder
    {
        Task SeedAsync(CancellationToken cancellationToken = default);
    }
}
