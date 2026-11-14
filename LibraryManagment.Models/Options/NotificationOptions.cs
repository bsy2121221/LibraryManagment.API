namespace LibraryManagement.Models.Options
{
    public class NotificationOptions
    {
        public const string SectionName = "Notifications";

        // How often the background job sends queued messages, and how many per run
        public int DispatchIntervalSeconds { get; set; } = 60;
        public int BatchSize { get; set; } = 50;

        // Added to 10-digit mobile numbers for SMS / WhatsApp / calls
        public string DefaultCountryCode { get; set; } = "91";

        public SmsOptions Sms { get; set; } = new();
        public WhatsAppOptions WhatsApp { get; set; } = new();
        public EmailOptions Email { get; set; } = new();
        public VoiceOptions Voice { get; set; } = new();
    }

    // "Mock" | "Msg91"
    public class SmsOptions
    {
        public string Provider { get; set; } = "Mock";
        public Msg91Options Msg91 { get; set; } = new();
    }

    public class Msg91Options
    {
        public string AuthKey { get; set; } = string.Empty;
        public string SenderId { get; set; } = string.Empty;
        public string DltTemplateId { get; set; } = string.Empty;

        // MSG91 Flow (template) id. The template must have one variable named "message".
        public string FlowTemplateId { get; set; } = string.Empty;
    }

    // "Mock" | "Meta"
    public class WhatsAppOptions
    {
        public string Provider { get; set; } = "Mock";
        public MetaWhatsAppOptions Meta { get; set; } = new();
    }

    public class MetaWhatsAppOptions
    {
        public string AccessToken { get; set; } = string.Empty;
        public string PhoneNumberId { get; set; } = string.Empty;
        public string ApiVersion { get; set; } = "v21.0";
    }

    // "Mock" | "Smtp"
    public class EmailOptions
    {
        public string Provider { get; set; } = "Mock";
        public SmtpOptions Smtp { get; set; } = new();
    }

    public class SmtpOptions
    {
        public string Host { get; set; } = "smtp.gmail.com";
        public int Port { get; set; } = 587;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;

        // Sender address; defaults to Username (for Gmail use an App Password, not the account password)
        public string FromEmail { get; set; } = string.Empty;
        public bool EnableSsl { get; set; } = true;
    }

    // "Mock" | "Twilio"
    public class VoiceOptions
    {
        public string Provider { get; set; } = "Mock";
        public TwilioOptions Twilio { get; set; } = new();
    }

    public class TwilioOptions
    {
        public string AccountSid { get; set; } = string.Empty;
        public string AuthToken { get; set; } = string.Empty;
        public string FromNumber { get; set; } = string.Empty;

        // Voice used to read the message, e.g. "en-IN" or "hi-IN"
        public string Language { get; set; } = "en-IN";
    }
}
