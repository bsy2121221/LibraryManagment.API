namespace LibraryManagement.Models.Options
{
    public class NotificationOptions
    {
        public const string SectionName = "Notifications";

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
    }
}
