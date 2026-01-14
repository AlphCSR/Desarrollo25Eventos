using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NotificationsMS.Domain.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace NotificationsMS.Infrastructure.Services
{
    public class SendGridEmailService : IEmailService
    {
        private readonly string _apiKey;
        private readonly string _fromEmail;

        public SendGridEmailService(IConfiguration configuration)
        {
            _apiKey = configuration["SendGrid:ApiKey"];
            _fromEmail = configuration["SendGrid:FromEmail"];
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            if (string.IsNullOrEmpty(_apiKey) || _apiKey.Equals("MOCK", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"[Dev Email] To: {to}, Subject: {subject}, Body: {body}");
                return;
            }

            var client = new SendGridClient(_apiKey);
            var from = new EmailAddress(_fromEmail, "TicketApp Notificaciones");
            var toAddress = new EmailAddress(to);
            var msg = MailHelper.CreateSingleEmail(from, toAddress, subject, body, body);
            
            await client.SendEmailAsync(msg);
        }
    }
}
