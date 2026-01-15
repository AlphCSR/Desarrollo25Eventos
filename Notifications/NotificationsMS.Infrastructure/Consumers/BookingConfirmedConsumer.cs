using BookingMS.Shared.Events;
using MassTransit;
using NotificationsMS.Domain.Interfaces;
using NotificationsMS.Application.Interfaces;
using System.Threading.Tasks;

namespace NotificationsMS.Infrastructure.Consumers
{
    public class BookingConfirmedConsumer : IConsumer<BookingConfirmedEvent>
    {
        private readonly IEmailService _emailService;
        private readonly IEmailTemplateService _templateService;
        private readonly INotifier _notifier;

        public BookingConfirmedConsumer(IEmailService emailService, IEmailTemplateService templateService, INotifier notifier)
        {
            _emailService = emailService;
            _templateService = templateService;
            _notifier = notifier;
        }

        public async Task Consume(ConsumeContext<BookingConfirmedEvent> context)
        {
            var message = context.Message;
            var (subject, body) = _templateService.GetBookingConfirmedTemplate(
                message.Language, 
                message.BookingId.ToString(), 
                message.EventName
            );

            System.Console.WriteLine($"[Notifications] Recibida BookingConfirmedEvent para Booking {message.BookingId} en {message.Language}. Enviando email a {message.Email}...");
            
            try 
            {
                await _emailService.SendEmailAsync(message.Email, subject, body);
                System.Console.WriteLine($"[Notifications] Email enviado a {message.Email}.");
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"[Notifications] Error enviando email: {ex.Message}");
            }
            
            try
            {
                var realTimeMsg = message.Language == "en"
                    ? $"Your booking has been confirmed successfully. # {message.BookingId.ToString().Substring(0, 8)}"
                    : $"Tu reserva se ha confirmado exitosamente. # {message.BookingId.ToString().Substring(0, 8)}";

                await _notifier.SendNotificationAsync(message.Email, realTimeMsg);
                System.Console.WriteLine($"[Notifications] Notificación real-time enviada al usuario {message.UserId}.");
            }
            catch (System.Exception ex)
            {
                 System.Console.WriteLine($"[Notifications] Error enviando notificación real-time: {ex.Message}");
            }
        }
    }
}