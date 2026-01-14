using BookingMS.Shared.Events;
using MassTransit;
using NotificationsMS.Domain.Interfaces;
using System.Threading.Tasks;

namespace NotificationsMS.Infrastructure.Consumers
{
    public class PaymentExpiringSoonConsumer : IConsumer<PaymentExpiringSoonEvent>
    {
        private readonly INotifier _notifier;
        private readonly IEmailService _emailService;

        public PaymentExpiringSoonConsumer(INotifier notifier, IEmailService emailService)
        {
            _notifier = notifier;
            _emailService = emailService;
        }

        public async Task Consume(ConsumeContext<PaymentExpiringSoonEvent> context)
        {
            var message = context.Message;
            
            var subject = "¡Tu reserva está por expirar!";
            var body = $@"Hola, 
            Te recordamos que tu reserva #{message.BookingId.ToString().Substring(0,8)} expira en aproximadamente {message.MinutesRemaining} minutos. 
            Por favor, completa tu pago pronto para asegurar tus asientos.";
            
            await _emailService.SendEmailAsync(message.Email, subject, body);

            await _notifier.SendNotificationAsync(message.Email, $"⚠️ Tu reserva #{message.BookingId.ToString().Substring(0, 8)} expira en {message.MinutesRemaining} min. ¡Págala ahora!");
            
            System.Console.WriteLine($"[Notifications] Sent expiration reminder to user {message.UserId} for booking {message.BookingId}");
        }
    }
}
