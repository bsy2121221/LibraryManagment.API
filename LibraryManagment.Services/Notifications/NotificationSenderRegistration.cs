using LibraryManagement.Models.Enums;
using LibraryManagement.Models.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LibraryManagement.Services.Notifications
{
    // Picks one sender per channel from appsettings.json:
    //   Notifications:Sms:Provider       "Mock" | "Msg91"
    //   Notifications:WhatsApp:Provider  "Mock" | "Meta"
    //   Notifications:Email:Provider     "Mock" | "Smtp"
    //   Notifications:Voice:Provider     "Mock" | "Twilio"
    // Switching a channel to a real provider needs only config changes (provider name + keys).
    public static class NotificationSenderRegistration
    {
        public static IServiceCollection AddNotificationSenders(this IServiceCollection services, IConfiguration configuration)
        {
            var options = configuration.GetSection(NotificationOptions.SectionName).Get<NotificationOptions>() ?? new NotificationOptions();

            AddSender(services, NotificationChannel.Sms, options.Sms.Provider, new()
            {
                ["msg91"] = s =>
                {
                    Require(options.Sms.Msg91.AuthKey, "Notifications:Sms:Msg91:AuthKey");
                    Require(options.Sms.Msg91.FlowTemplateId, "Notifications:Sms:Msg91:FlowTemplateId");
                    s.AddHttpClient<Msg91SmsSender>(c => c.Timeout = TimeSpan.FromSeconds(30));
                    s.AddTransient<INotificationSender>(sp => sp.GetRequiredService<Msg91SmsSender>());
                }
            });

            AddSender(services, NotificationChannel.WhatsApp, options.WhatsApp.Provider, new()
            {
                ["meta"] = s =>
                {
                    Require(options.WhatsApp.Meta.AccessToken, "Notifications:WhatsApp:Meta:AccessToken");
                    Require(options.WhatsApp.Meta.PhoneNumberId, "Notifications:WhatsApp:Meta:PhoneNumberId");
                    // TemplateName is required so messages go out without the student having to reply first
                    Require(options.WhatsApp.Meta.TemplateName, "Notifications:WhatsApp:Meta:TemplateName");
                    s.AddHttpClient<MetaWhatsAppSender>(c => c.Timeout = TimeSpan.FromSeconds(30));
                    s.AddTransient<INotificationSender>(sp => sp.GetRequiredService<MetaWhatsAppSender>());
                }
            });

            AddSender(services, NotificationChannel.Email, options.Email.Provider, new()
            {
                ["smtp"] = s =>
                {
                    Require(options.Email.Smtp.Host, "Notifications:Email:Smtp:Host");
                    Require(options.Email.Smtp.Username, "Notifications:Email:Smtp:Username");
                    s.AddSingleton<INotificationSender, SmtpEmailSender>();
                }
            });

            AddSender(services, NotificationChannel.Call, options.Voice.Provider, new()
            {
                ["twilio"] = s =>
                {
                    Require(options.Voice.Twilio.AccountSid, "Notifications:Voice:Twilio:AccountSid");
                    Require(options.Voice.Twilio.AuthToken, "Notifications:Voice:Twilio:AuthToken");
                    Require(options.Voice.Twilio.FromNumber, "Notifications:Voice:Twilio:FromNumber");
                    s.AddHttpClient<TwilioVoiceSender>(c => c.Timeout = TimeSpan.FromSeconds(30));
                    s.AddTransient<INotificationSender>(sp => sp.GetRequiredService<TwilioVoiceSender>());
                },
                ["fast2sms"] = s =>
                {
                    Require(options.Voice.Fast2Sms.ApiKey, "Notifications:Voice:Fast2Sms:ApiKey");
                    Require(options.Voice.Fast2Sms.FileId.ToString(), "Notifications:Voice:Fast2Sms:FileId");
                    s.AddHttpClient<Fast2SmsVoiceSender>(c => c.Timeout = TimeSpan.FromSeconds(30));
                    s.AddTransient<INotificationSender>(sp => sp.GetRequiredService<Fast2SmsVoiceSender>());
                }
            });

            services.AddScoped<INotificationDispatcher, NotificationDispatcher>();
            return services;
        }

        // Provider names in use per channel, shown on the admin Messages screen
        public static Dictionary<NotificationChannel, string> ConfiguredProviders(NotificationOptions options) => new()
        {
            [NotificationChannel.Sms] = options.Sms.Provider,
            [NotificationChannel.WhatsApp] = options.WhatsApp.Provider,
            [NotificationChannel.Email] = options.Email.Provider,
            [NotificationChannel.Call] = options.Voice.Provider
        };

        private static void AddSender(IServiceCollection services, NotificationChannel channel, string? provider,
            Dictionary<string, Action<IServiceCollection>> realProviders)
        {
            var key = (provider ?? "Mock").Trim().ToLowerInvariant();
            if (key == "mock")
            {
                services.AddSingleton<INotificationSender>(sp =>
                    new MockNotificationSender(channel, sp.GetRequiredService<ILogger<MockNotificationSender>>()));
                return;
            }

            if (!realProviders.TryGetValue(key, out var register))
            {
                throw new InvalidOperationException(
                    $"Unknown notification provider '{provider}' for {channel}. Use 'Mock' or {string.Join(", ", realProviders.Keys.Select(k => $"'{k}'"))}.");
            }
            register(services);
        }

        private static void Require(string? value, string setting)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"{setting} must be set in appsettings.json for this notification provider.");
            }
        }
    }
}
