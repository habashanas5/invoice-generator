using Invoice_Generator.AppSettings;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;
namespace Invoice_Generator.Infrastructures.Emails
{
    public class SMTPEmailService : IEmailSender
    {
        private readonly SmtpConfiguration _smtpConfiguration;

        public SMTPEmailService(IOptions<SmtpConfiguration> smtpConfiguration)
        {
            _smtpConfiguration = smtpConfiguration.Value;
        }
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_smtpConfiguration.FromName, _smtpConfiguration.FromAddress));
            message.To.Add(new MailboxAddress(email, email));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlMessage
            };

            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                try
                {
                    await client.ConnectAsync(_smtpConfiguration.Host, _smtpConfiguration.Port, true);
                    await client.AuthenticateAsync(_smtpConfiguration.UserName, _smtpConfiguration.Password);
                    await client.SendAsync(message);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Error sending email: {ex.Message}", ex);
                }
                finally
                {
                    await client.DisconnectAsync(true);
                }
            }
        }
    }
}