using LibraryManagement.Models.Entities;
using System.Globalization;

namespace LibraryManagement.Services.Settings
{
    // How each known setting is shown and validated on the admin Settings screen.
    // A setting in dbo.Settings that is not listed here is still editable, validated by its DataType only.
    public sealed record SettingDefinition(string Key, string Label, string Group, string DataType, decimal? Min = null, decimal? Max = null);

    public static class SettingDefinitions
    {
        public const string General = "General";
        public const string FeesAndBooking = "Fees & booking";
        public const string Billing = "Billing & overdue";

        public static readonly IReadOnlyList<SettingDefinition> All =
        [
            new(SettingKeys.LibraryName, "Library name", General, "string", 2, 100),
            new(SettingKeys.MonthlyFee, "Monthly fee (₹)", FeesAndBooking, "decimal", 1, 100000),
            new(SettingKeys.SeatHoldMinutes, "Seat hold time while paying (minutes)", FeesAndBooking, "int", 1, 60),
            new(SettingKeys.InvoiceGenerationDaysBefore, "Create next month's fee this many days before the due date", Billing, "int", 0, 28),
            new(SettingKeys.GraceDays, "Grace days after the due date before a fee is overdue", Billing, "int", 0, 30),
            new(SettingKeys.AutoReleaseAfterOverdueDays, "Release the seat automatically after this many overdue days (0 = never)", Billing, "int", 0, 365),
        ];

        private static readonly Dictionary<string, SettingDefinition> ByKey =
            All.ToDictionary(d => d.Key, StringComparer.OrdinalIgnoreCase);

        public static SettingDefinition For(Setting setting) =>
            ByKey.TryGetValue(setting.Key, out var definition)
                ? definition
                : new SettingDefinition(setting.Key, setting.Key, "Other", setting.DataType);

        public static int Order(string key)
        {
            for (var i = 0; i < All.Count; i++)
            {
                if (string.Equals(All[i].Key, key, StringComparison.OrdinalIgnoreCase)) return i;
            }
            return int.MaxValue;
        }

        // Returns the value normalised for storage (trimmed, invariant number format), or an error message
        public static (string? Value, string? Error) Validate(SettingDefinition definition, string? raw)
        {
            var value = raw?.Trim() ?? string.Empty;

            switch (definition.DataType.ToLowerInvariant())
            {
                case "int":
                    if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i))
                        return (null, $"{definition.Label} must be a whole number.");
                    if (OutOfRange(i, definition))
                        return (null, $"{definition.Label} must be between {definition.Min} and {definition.Max}.");
                    return (i.ToString(CultureInfo.InvariantCulture), null);

                case "decimal":
                    if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var d))
                        return (null, $"{definition.Label} must be a number.");
                    if (decimal.Round(d, 2) != d)
                        return (null, $"{definition.Label} can have at most 2 decimal places.");
                    if (OutOfRange(d, definition))
                        return (null, $"{definition.Label} must be between {definition.Min} and {definition.Max}.");
                    return (d.ToString(CultureInfo.InvariantCulture), null);

                case "bool":
                    if (!bool.TryParse(value, out var b))
                        return (null, $"{definition.Label} must be true or false.");
                    return (b ? "true" : "false", null);

                default:
                    // For text, Min/Max are lengths
                    if (definition.Min is { } min && value.Length < min)
                        return (null, $"{definition.Label} must be at least {min} characters.");
                    if (definition.Max is { } max && value.Length > max)
                        return (null, $"{definition.Label} must be at most {max} characters.");
                    return (value, null);
            }
        }

        private static bool OutOfRange(decimal value, SettingDefinition d) =>
            (d.Min is { } min && value < min) || (d.Max is { } max && value > max);
    }
}
