using LibraryManagement.Models.Enums;
using LibraryManagement.Models.Options;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security;
using System.Text;
using System.Text.Json;

namespace LibraryManagement.Services.Notifications
{
    // Real providers called over HTTP. Each is switched on only by its "Provider" value in appsettings.json.
    // Check each provider's own rules before going live (see the notes on each class).
    internal static class HttpSendHelper
    {
        public static async Task<JsonDocument> SendAsync(HttpClient client, HttpRequestMessage request, string provider, CancellationToken cancellationToken)
        {
            using var response = await client.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                // 4xx = the request itself is wrong (number, template, credentials): retrying will not help
                var permanent = (int)response.StatusCode is >= 400 and < 500 and not 408 and not 429;
                throw new NotificationSendException(
                    $"{provider} returned {(int)response.StatusCode}: {Truncate(body, 300)}", permanent);
            }

            return string.IsNullOrWhiteSpace(body) ? JsonDocument.Parse("{}") : JsonDocument.Parse(body);
        }

        public static string Truncate(string value, int max) => value.Length <= max ? value : value[..max] + "…";
    }

    // "Notifications:Sms:Provider": "Msg91" - MSG91 Flow API (https://docs.msg91.com).
    // Indian SMS needs a DLT-approved template: create a Flow whose template has one variable named "message",
    // and put its id in Msg91:FlowTemplateId.
    public class Msg91SmsSender : INotificationSender
    {
        private readonly HttpClient _httpClient;
        private readonly NotificationOptions _options;

        public Msg91SmsSender(HttpClient httpClient, IOptions<NotificationOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public NotificationChannel Channel => NotificationChannel.Sms;

        public string ProviderName => "Msg91";

        public async Task<string?> SendAsync(OutgoingMessage message, CancellationToken cancellationToken = default)
        {
            var msg91 = _options.Sms.Msg91;
            var payload = new
            {
                template_id = msg91.FlowTemplateId,
                short_url = "0",
                recipients = new[]
                {
                    new Dictionary<string, string>
                    {
                        ["mobiles"] = PhoneNumbers.WithCountryCode(message.Recipient, _options.DefaultCountryCode),
                        ["message"] = message.Body
                    }
                }
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://control.msg91.com/api/v5/flow")
            {
                Content = JsonContent.Create(payload)
            };
            request.Headers.Add("authkey", msg91.AuthKey);

            using var result = await HttpSendHelper.SendAsync(_httpClient, request, ProviderName, cancellationToken);
            var root = result.RootElement;
            if (root.TryGetProperty("type", out var type) && type.GetString() == "error")
            {
                throw new NotificationSendException($"Msg91: {root.GetRawText()}", permanent: true);
            }
            return root.TryGetProperty("message", out var id) ? id.ToString() : null;
        }
    }

    // "Notifications:WhatsApp:Provider": "Meta" - WhatsApp Cloud API.
    // Uses an approved UTILITY template (Meta:TemplateName) so students get messages even if they never
    // replied. Free-text is only used when TemplateName is blank (and only works inside Meta's 24h window).
    public class MetaWhatsAppSender : INotificationSender
    {
        private readonly HttpClient _httpClient;
        private readonly NotificationOptions _options;

        public MetaWhatsAppSender(HttpClient httpClient, IOptions<NotificationOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public NotificationChannel Channel => NotificationChannel.WhatsApp;

        public string ProviderName => "Meta";

        public async Task<string?> SendAsync(OutgoingMessage message, CancellationToken cancellationToken = default)
        {
            var meta = _options.WhatsApp.Meta;
            var to = PhoneNumbers.WithCountryCode(message.Recipient, _options.DefaultCountryCode);

            object payload = string.IsNullOrWhiteSpace(meta.TemplateName)
                ? BuildFreeTextPayload(to, message.Body)
                : BuildTemplatePayload(to, meta, message.Body);

            using var request = new HttpRequestMessage(HttpMethod.Post,
                $"https://graph.facebook.com/{meta.ApiVersion}/{meta.PhoneNumberId}/messages")
            {
                Content = JsonContent.Create(payload)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", meta.AccessToken);

            using var result = await HttpSendHelper.SendAsync(_httpClient, request, ProviderName, cancellationToken);
            return result.RootElement.TryGetProperty("messages", out var messages) && messages.GetArrayLength() > 0
                ? messages[0].GetProperty("id").GetString()
                : null;
        }

        private static object BuildFreeTextPayload(string to, string body) => new
        {
            messaging_product = "whatsapp",
            to,
            type = "text",
            text = new { preview_url = false, body }
        };

        // Template body must contain exactly one variable {{1}} filled with the rendered library message.
        private static object BuildTemplatePayload(string to, MetaWhatsAppOptions meta, string body) => new
        {
            messaging_product = "whatsapp",
            to,
            type = "template",
            template = new
            {
                name = meta.TemplateName.Trim(),
                language = new { code = string.IsNullOrWhiteSpace(meta.TemplateLanguage) ? "en" : meta.TemplateLanguage.Trim() },
                components = new object[]
                {
                    new
                    {
                        type = "body",
                        parameters = new object[]
                        {
                            new { type = "text", text = ToTemplateParameter(body) }
                        }
                    }
                }
            }
        };

        // Meta rejects empty params, leading/trailing spaces, and newlines inside variables.
        private static string ToTemplateParameter(string value)
        {
            var cleaned = string.Join(' ', value
                .Replace('\r', ' ')
                .Replace('\n', ' ')
                .Split(' ', StringSplitOptions.RemoveEmptyEntries));

            if (cleaned.Length == 0)
            {
                throw new NotificationSendException("WhatsApp template message body is empty.", permanent: true);
            }

            return cleaned.Length <= 1024 ? cleaned : cleaned[..1024];
        }
    }

    // "Notifications:Voice:Provider": "Twilio" - places a phone call that reads the message aloud.
    // The FromNumber must be a Twilio voice number; calls to India need the geo permission enabled in Twilio.
    public class TwilioVoiceSender : INotificationSender
    {
        private readonly HttpClient _httpClient;
        private readonly NotificationOptions _options;

        public TwilioVoiceSender(HttpClient httpClient, IOptions<NotificationOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public NotificationChannel Channel => NotificationChannel.Call;

        public string ProviderName => "Twilio";

        public async Task<string?> SendAsync(OutgoingMessage message, CancellationToken cancellationToken = default)
        {
            var twilio = _options.Voice.Twilio;
            var twiml = $"<Response><Say language=\"{SecurityElement.Escape(twilio.Language)}\">{SecurityElement.Escape(message.Body)}</Say></Response>";

            using var request = new HttpRequestMessage(HttpMethod.Post,
                $"https://api.twilio.com/2010-04-01/Accounts/{WebUtility.UrlEncode(twilio.AccountSid)}/Calls.json")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["To"] = PhoneNumbers.ToE164(message.Recipient, _options.DefaultCountryCode),
                    ["From"] = twilio.FromNumber,
                    ["Twiml"] = twiml
                })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{twilio.AccountSid}:{twilio.AuthToken}")));

            using var result = await HttpSendHelper.SendAsync(_httpClient, request, ProviderName, cancellationToken);
            return result.RootElement.TryGetProperty("sid", out var sid) ? sid.GetString() : null;
        }
    }

    public class Fast2SmsVoiceSender : INotificationSender
    {
        private readonly HttpClient _httpClient;
        private readonly NotificationOptions _options;
        public Fast2SmsVoiceSender(HttpClient httpClient, IOptions<NotificationOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }
        public NotificationChannel Channel => NotificationChannel.Call;
        public string ProviderName => "Fast2Sms";
        public async Task<string?> SendAsync(OutgoingMessage message, CancellationToken cancellationToken = default)
        {
            var cfg = _options.Voice.Fast2Sms;
            if (cfg.FileId <= 0)
                throw new NotificationSendException("Notifications:Voice:Fast2Sms:FileId must be a positive approved file id.", permanent: true);
            // Fast2SMS wants 10-digit Indian numbers
            var digits = new string(message.Recipient.Where(char.IsDigit).ToArray());
            if (digits.Length == 12 && digits.StartsWith("91")) digits = digits[2..];
            if (digits.Length == 11 && digits.StartsWith('0')) digits = digits[1..];
            if (digits.Length != 10)
                throw new NotificationSendException($"'{message.Recipient}' is not a 10-digit Indian mobile.", permanent: true);
            using var request = new HttpRequestMessage(HttpMethod.Post, "https://www.fast2sms.com/dev/voice/bulk")
            {
                Content = JsonContent.Create(new { numbers = digits, file_id = cfg.FileId })
            };
            request.Headers.TryAddWithoutValidation("authorization", cfg.ApiKey);
            using var result = await HttpSendHelper.SendAsync(_httpClient, request, ProviderName, cancellationToken);
            return result.RootElement.TryGetProperty("request_id", out var id) ? id.GetString() : null;
        }
    }
}
