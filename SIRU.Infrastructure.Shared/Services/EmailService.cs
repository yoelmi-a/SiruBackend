using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using SIRU.Core.Application.Dtos.Emails;
using SIRU.Core.Application.Interfaces.Shared;
using SIRU.Core.Domain.Settings;

namespace SIRU.Infrastructure.Shared.Services
{
    public class EmailService : IEmailService
    {
        private readonly MailSettings _mailSettings;

        public EmailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }

        public async Task SendAsync(EmailRequestDto emailRequest)
        {
            if (!emailRequest.To.Any())
            {
                throw new ArgumentException("The email must have a recipient");
            }

            MimeMessage email = new()
            {
                Sender = MailboxAddress.Parse(_mailSettings.EmailFrom ?? ""),
                Subject = emailRequest.Subject ?? "",
            };

            foreach (var toItem in emailRequest.To)
            {
                email.To.Add(MailboxAddress.Parse(toItem));
            }

            BodyBuilder builder = new()
            {
                HtmlBody = emailRequest.HtmlBody
            };

            email.Body = builder.ToMessageBody();

            using SmtpClient smtpClient = new();
            await smtpClient.ConnectAsync(_mailSettings.SmtpHost, _mailSettings.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            await smtpClient.AuthenticateAsync(_mailSettings.SmtpUser, _mailSettings.SmtpPass);
            await smtpClient.SendAsync(email);
            await smtpClient.DisconnectAsync(true);
        }
    }
}
