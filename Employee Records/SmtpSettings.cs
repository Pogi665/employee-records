using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;

namespace Employee_Records.Services
{
    public class SmtpSettings
    {
        public string Host { get; set; } = "";
        public int Port { get; set; } = 587;
        public bool UseStartTls { get; set; } = true;
        public string User { get; set; } = "";
        public string Password { get; set; } = "";
        public string From { get; set; } = "";
        public string FromName { get; set; } = "";
    }

    public class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpSettings _settings;

        public SmtpEmailSender(IOptions<SmtpSettings> options)
        {
            _settings = options.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.FromName ?? _settings.From, _settings.From));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlMessage };
            message.Body = bodyBuilder.ToMessageBody();

            using var protocolLogger = new MailKit.ProtocolLogger("mailkit-protocol.log");
            using var client = new MailKit.Net.Smtp.SmtpClient(protocolLogger);
            // Accept all SSL certificates for dev only
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;
            var secureOption = _settings.UseStartTls ? MailKit.Security.SecureSocketOptions.StartTls : MailKit.Security.SecureSocketOptions.Auto;
            await client.ConnectAsync(_settings.Host, _settings.Port, secureOption).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(_settings.User))
            {
                await client.AuthenticateAsync(_settings.User, _settings.Password).ConfigureAwait(false);
            }
            await client.SendAsync(message).ConfigureAwait(false);
            await client.DisconnectAsync(true).ConfigureAwait(false);
        }
    }
}