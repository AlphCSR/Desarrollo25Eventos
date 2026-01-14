using BookingMS.Shared.Events;
using MassTransit;
using NotificationsMS.Domain.Interfaces;
using NotificationsMS.Application.Interfaces;
using System.Threading.Tasks;

namespace NotificationsMS.Infrastructure.Consumers
{
    public class BookingCancelledConsumer : IConsumer<BookingCancelledEvent>
    {
        private readonly INotifier _notifier;
        private readonly IEmailService _emailService;
        private readonly IEmailTemplateService _templateService;

        public BookingCancelledConsumer(INotifier notifier, IEmailService emailService, IEmailTemplateService templateService)
        {
            _notifier = notifier;
            _emailService = emailService;
            _templateService = templateService;
        }

        public async Task Consume(ConsumeContext<BookingCancelledEvent> context)
        {
            var message = context.Message;
            
            var (subject, body) = _templateService.GetBookingCancelledTemplate(
                message.Language,
                message.BookingId.ToString(),
                message.EventName
            );
            
            await _emailService.SendEmailAsync(message.Email, subject, body);

            var realTimeMsg = message.Language == "en" 
                ? $"Your booking has been cancelled. Reason: {message.Reason}. # {message.BookingId.ToString().Substring(0, 8)}"
                : $"Tu reserva se ha cancelado. Motivo: {message.Reason}. # {message.BookingId.ToString().Substring(0, 8)}";

            await _notifier.SendNotificationAsync(message.Email, realTimeMsg);
            
            System.Console.WriteLine($"[Notifications] User {message.UserId} notificado sobre cancelacion de reserva {message.BookingId} en {message.Language}.");
        }
    }
}
