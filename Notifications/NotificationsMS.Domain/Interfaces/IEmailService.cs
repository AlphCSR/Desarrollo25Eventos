using System.Threading.Tasks;

namespace NotificationsMS.Domain.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }
}
