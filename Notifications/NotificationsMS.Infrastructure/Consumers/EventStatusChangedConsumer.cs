using EventsMS.Shared.Events;
using MassTransit;
using NotificationsMS.Domain.Interfaces;
using System.Threading.Tasks;

namespace NotificationsMS.Infrastructure.Consumers
{
    public class EventStatusChangedConsumer : IConsumer<EventStatusChangedEvent>
    {
        private readonly INotifier _notifier;

        public EventStatusChangedConsumer(INotifier notifier)
        {
            _notifier = notifier;
        }

        public async Task Consume(ConsumeContext<EventStatusChangedEvent> context)
        {
            var msg = context.Message;
            
            await _notifier.BroadcastSeatUpdateAsync(new {
                type = "EventStatusChanged",
                eventId = msg.EventId,
                newStatus = msg.NewStatus.ToString(),
                timestamp = msg.Timestamp
            });
            
            System.Console.WriteLine($"[Notifications] Broadcasted status change for Event {msg.EventId} to All. New Status: {msg.NewStatus}");
        }
    }
}
