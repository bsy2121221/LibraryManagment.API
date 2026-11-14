using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace LibraryManagement.Services.Notifications
{
    // Fills {Placeholders} in a message template from the row's DataJson (+ LibraryName).
    // Unknown placeholders become empty text, so a typo in a template never shows "{Seta}" to a student.
    public static partial class TemplateRenderer
    {
        [GeneratedRegex(@"\{(\w+)\}")]
        private static partial Regex PlaceholderPattern();

        public static Dictionary<string, string> ParseData(string? dataJson, string libraryName)
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrWhiteSpace(dataJson))
            {
                using var document = JsonDocument.Parse(dataJson);
                if (document.RootElement.ValueKind == JsonValueKind.Object)
                {
                    foreach (var property in document.RootElement.EnumerateObject())
                    {
                        values[property.Name] = property.Value.ValueKind switch
                        {
                            JsonValueKind.String => property.Value.GetString() ?? string.Empty,
                            JsonValueKind.Null => string.Empty,
                            _ => property.Value.GetRawText()
                        };
                    }
                }
            }
            values["LibraryName"] = libraryName;
            return values;
        }

        // htmlEncodeValues: for email bodies (HTML), so a student's name cannot inject markup
        public static string Render(string? template, IReadOnlyDictionary<string, string> values, bool htmlEncodeValues = false)
        {
            if (string.IsNullOrEmpty(template))
            {
                return string.Empty;
            }

            return PlaceholderPattern().Replace(template, match =>
            {
                var value = values.TryGetValue(match.Groups[1].Value, out var v) ? v : string.Empty;
                return htmlEncodeValues ? WebUtility.HtmlEncode(value) : value;
            });
        }
    }
}
