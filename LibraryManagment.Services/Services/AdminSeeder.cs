using LibraryManagement.Auth.Services;
using LibraryManagement.DataAccess.IRepositories;
using LibraryManagement.Models.Entities;
using LibraryManagement.Models.Enums;
using LibraryManagement.Models.Options;
using LibraryManagement.Services.IServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LibraryManagement.Services.Services
{
    // Creates the first admin from the AdminSeed config section. Does nothing once any admin exists.
    public class AdminSeeder : IAdminSeeder
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly AdminSeedOptions _options;
        private readonly ILogger<AdminSeeder> _logger;

        public AdminSeeder(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IOptions<AdminSeedOptions> options,
            ILogger<AdminSeeder> logger)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _options = options.Value;
            _logger = logger;
        }

        public async Task SeedAsync(CancellationToken cancellationToken = default)
        {
            if (await _userRepository.AnyAdminExistsAsync(cancellationToken))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(_options.Mobile) || string.IsNullOrWhiteSpace(_options.Password))
            {
                _logger.LogWarning("No admin user exists and AdminSeed:Mobile / AdminSeed:Password are not configured");
                return;
            }

            var adminId = await _userRepository.CreateAsync(new User
            {
                FullName = _options.FullName,
                Mobile = _options.Mobile.Trim(),
                PasswordHash = _passwordHasher.Hash(_options.Password),
                Role = UserRole.Admin,
                IsMobileVerified = true,
                IsActive = true
            }, cancellationToken);

            _logger.LogInformation("Seeded admin user {UserId} with mobile {Mobile}", adminId, _options.Mobile);
        }
    }
}
