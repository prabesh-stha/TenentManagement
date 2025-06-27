using System.Net.Mail;
using System.Net;

namespace TenentManagement.Services.Mail
{
    public class MailService(IConfiguration configuration)
    {
        private readonly IConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        public void Send(string to, string subject, string body)
        {
            if (_configuration != null)
            {
                var smtpHost = _configuration["Email:Smtp:Host"];
                var smtpPort = _configuration["Email:Smtp:Port"];
                var smtpUsername = _configuration["Email:Smtp:Username"];
                var smtpPassword = _configuration["Email:Smtp:Password"];
                var smtpFrom = _configuration["Email:Smtp:From"];

                if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpPort) ||
                    string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword) ||
                    string.IsNullOrEmpty(smtpFrom))
                {
                    throw new InvalidOperationException("SMTP configuration is incomplete.");
                }

                var smtpClient = new SmtpClient
                {
                    Host = smtpHost,
                    Port = int.Parse(smtpPort),
                    EnableSsl = true,
                    Credentials = new NetworkCredential(smtpUsername, smtpPassword)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpFrom),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(to);
                smtpClient.Send(mailMessage);
            }
        }
    }
}