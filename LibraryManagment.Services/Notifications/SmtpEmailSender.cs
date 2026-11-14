using LibraryManagement.Models.Enums;
using LibraryManagement.Models.Options;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace LibraryManagement.Services.Notifications
{
    // "Notifications:Email:Provider": "Smtp". Works with Gmail (smtp.gmail.com:587 + an App Password),
    // Outlook, Zoho, or any SMTP server.
    public class SmtpEmailSender : INotificationSender
    {
        private readonly SmtpOptions _options;

        public SmtpEmailSender(IOptions<NotificationOptions> options)
        {
            _options = options.Value.Email.Smtp;
        }

        public NotificationChannel Channel => NotificationChannel.Email;

        public string ProviderName => "Smtp";

        public async Task<string?> SendAsync(OutgoingMessage message, CancellationToken cancellationToken = default)
        {
            MailAddress to;
            try
            {
                to = new MailAddress(message.Recipient);
            }
            catch (FormatException ex)
            {
                throw new NotificationSendException($"'{message.Recipient}' is not a valid email address.", permanent: true, ex);
            }

            var fromAddress = string.IsNullOrWhiteSpace(_options.FromEmail) ? _options.Username : _options.FromEmail;
            using var mail = new MailMessage
            {
                From = new MailAddress(fromAddress, _options.FromName),
                Subject = message.Subject ?? string.Empty,
                Body = message.Body,
                IsBodyHtml = true
            };
            mail.To.Add(to);

            using var client = new SmtpClient(_options.Host, _options.Port)
            {
                EnableSsl = _options.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(_options.Username, _options.Password)
            };

            try
            {
                await client.SendMailAsync(mail, cancellationToken);
            }
            catch (SmtpFailedRecipientException ex)
            {
                throw new NotificationSendException($"Email rejected for {message.Recipient}: {ex.StatusCode}", permanent: true, ex);
            }

            return mail.Headers["Message-ID"];
        }
    }
}
