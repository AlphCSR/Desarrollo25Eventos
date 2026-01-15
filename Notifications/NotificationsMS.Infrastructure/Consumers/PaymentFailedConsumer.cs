using BookingMS.Shared.Events;
using MassTransit;
using NotificationsMS.Domain.Interfaces;
using System.Threading.Tasks;

namespace NotificationsMS.Infrastructure.Consumers
{
    public class PaymentFailedConsumer : IConsumer<PaymentFailedEvent>
    {
        private readonly IEmailService _emailService;
        private readonly INotifier _notifier;

        public PaymentFailedConsumer(IEmailService emailService, INotifier notifier)
        {
            _emailService = emailService;
            _notifier = notifier;
        }

        public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
        {
            var message = context.Message;
            var subject = "Pago Fallido";
            var body = $"Hola, hubo un problema con el pago de tu reserva {message.BookingId}. Razón: {message.Reason}. Por favor intenta nuevamente.";

            await _emailService.SendEmailAsync(message.Email, subject, body);
            await _notifier.SendNotificationAsync(message.Email, $"Problema con el pago de tu reserva: {message.Reason}. # {message.BookingId.ToString().Substring(0, 8)}");
        }
    }
}