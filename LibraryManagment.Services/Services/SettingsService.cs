using LibraryManagement.DataAccess.IRepositories;
using LibraryManagement.Models.DTOs.Settings;
using LibraryManagement.Models.Entities;
using LibraryManagement.Models.Exceptions;
using LibraryManagement.Models.Options;
using LibraryManagement.Services.IServices;
using LibraryManagement.Services.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace LibraryManagement.Services.Services
{
    public class SettingsService : ISettingsService
    {
        // Used when a setting row is missing or not a valid number
        private const string DefaultLibraryName = "My Library";
        private const decimal DefaultMonthlyFee = 1000m;
        private const int DefaultSeatHoldMinutes = 7;

        private readonly ISettingRepository _settingRepository;
        private readonly PaymentOptions _paymentOptions;
        private readonly ILogger<SettingsService> _logger;

        public SettingsService(ISettingRepository settingRepository, IOptions<PaymentOptions> paymentOptions, ILogger<SettingsService> logger)
        {
            _settingRepository = settingRepository;
            _paymentOptions = paymentOptions.Value;
            _logger = logger;
        }

        public async Task<PublicSettingsDto> GetPublicAsync(CancellationToken cancellationToken = default)
        {
            var settings = await LoadAsync(cancellationToken);
            return new PublicSettingsDto
            {
                LibraryName = LibraryName(settings),
                MonthlyFee = MonthlyFee(settings),
                SeatHoldMinutes = SeatHoldMinutes(settings),
                Currency = _paymentOptions.Currency
            };
        }

        public async Task<string> GetLibraryNameAsync(CancellationToken cancellationToken = default)
            => LibraryName(await LoadAsync(cancellationToken));

        public async Task<decimal> GetMonthlyFeeAsync(CancellationToken cancellationToken = default)
            => MonthlyFee(await LoadAsync(cancellationToken));

        public async Task<int> GetSeatHoldMinutesAsync(CancellationToken cancellationToken = default)
            => SeatHoldMinutes(await LoadAsync(cancellationToken));

        public async Task<BillingSettings> GetBillingSettingsAsync(CancellationToken cancellationToken = default)
        {
            var settings = await LoadAsync(cancellationToken);
            return new BillingSettings(
                MonthlyFee(settings),
                IntSetting(settings, SettingKeys.InvoiceGenerationDaysBefore, 5, 0, 28),
                IntSetting(settings, SettingKeys.GraceDays, 3, 0, 30),
                IntSetting(settings, SettingKeys.AutoReleaseAfterOverdueDays, 0, 0, 365));
        }

        private static int IntSetting(Dictionary<string, string> s, string key, int fallback, int min, int max)
            => s.TryGetValue(key, out var value)
               && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number) && number >= min && number <= max
                ? number
                : fallback;

        public async Task<IReadOnlyList<AdminSettingDto>> GetAllForAdminAsync(CancellationToken cancellationToken = default)
        {
            var rows = await _settingRepository.GetAllAsync(cancellationToken);
            return rows
                .OrderBy(s => SettingDefinitions.Order(s.Key))
                .ThenBy(s => s.Key)
                .Select(s =>
                {
                    var definition = SettingDefinitions.For(s);
                    return new AdminSettingDto
                    {
                        Key = s.Key,
                        Label = definition.Label,
                        Value = s.Value,
                        DataType = definition.DataType,
                        Description = s.Description,
                        Group = definition.Group,
                        Min = definition.Min,
                        Max = definition.Max,
                        UpdatedAt = s.UpdatedAt
                    };
                })
                .ToList();
        }

        public async Task<IReadOnlyList<AdminSettingDto>> UpdateAsync(int adminUserId, IDictionary<string, string> values, CancellationToken cancellationToken = default)
        {
            var existing = (await _settingRepository.GetAllAsync(cancellationToken))
                .ToDictionary(s => s.Key, StringComparer.OrdinalIgnoreCase);

            var normalized = new Dictionary<string, string>();
            var errors = new List<string>();
            foreach (var (key, raw) in values)
            {
                if (!existing.TryGetValue(key, out var setting))
                {
                    errors.Add($"Unknown setting \"{key}\".");
                    continue;
                }

                var (value, error) = SettingDefinitions.Validate(SettingDefinitions.For(setting), raw);
                if (error is not null)
                {
                    errors.Add(error);
                }
                else
                {
                    normalized[setting.Key] = value!;
                }
            }

            if (errors.Count > 0)
            {
                throw new BadRequestException(string.Join(" ", errors));
            }

            await _settingRepository.UpdateManyAsync(normalized, adminUserId, cancellationToken);
            _logger.LogInformation("Admin {AdminUserId} updated settings {Keys}", adminUserId, string.Join(", ", normalized.Keys));

            return await GetAllForAdminAsync(cancellationToken);
        }

        private async Task<Dictionary<string, string>> LoadAsync(CancellationToken cancellationToken)
        {
            var rows = await _settingRepository.GetAllAsync(cancellationToken);
            return rows.ToDictionary(s => s.Key, s => s.Value, StringComparer.OrdinalIgnoreCase);
        }

        private static string LibraryName(Dictionary<string, string> s)
            => s.TryGetValue(SettingKeys.LibraryName, out var name) && !string.IsNullOrWhiteSpace(name) ? name : DefaultLibraryName;

        private static decimal MonthlyFee(Dictionary<string, string> s)
            => s.TryGetValue(SettingKeys.MonthlyFee, out var value)
               && decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var fee) && fee > 0
                ? fee
                : DefaultMonthlyFee;

        private static int SeatHoldMinutes(Dictionary<string, string> s)
            => s.TryGetValue(SettingKeys.SeatHoldMinutes, out var value)
               && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var minutes) && minutes is > 0 and <= 60
                ? minutes
                : DefaultSeatHoldMinutes;
    }
}
