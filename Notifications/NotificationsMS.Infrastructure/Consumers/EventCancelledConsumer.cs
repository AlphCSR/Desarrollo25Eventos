using EventsMS.Shared.Events;
using MassTransit;
using NotificationsMS.Domain.Interfaces;
using System.Threading.Tasks;

namespace NotificationsMS.Infrastructure.Consumers
{
    public class EventCancelledConsumer : IConsumer<EventCancelledEvent>
    {
        private readonly INotifier _notifier;

        public EventCancelledConsumer(INotifier notifier)
        {
            _notifier = notifier;
        }

        public async Task Consume(ConsumeContext<EventCancelledEvent> context)
        {
            var message = context.Message;
            
            System.Console.WriteLine($"[Notifications] Evento Cancelado: {message.Title} ({message.EventId}). Razón: {message.Reason}");
            
            await Task.CompletedTask;
        }
    }
}
