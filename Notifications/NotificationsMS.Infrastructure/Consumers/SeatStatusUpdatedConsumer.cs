using MassTransit;
using NotificationsMS.Domain.Interfaces;
using SeatingMS.Shared.Events;
using System.Threading.Tasks;

namespace NotificationsMS.Infrastructure.Consumers
{
    public class SeatStatusUpdatedConsumer : IConsumer<SeatStatusUpdatedEvent>
    {
        private readonly INotifier _notifier;

        public SeatStatusUpdatedConsumer(INotifier notifier)
        {
            _notifier = notifier;
        }

        public async Task Consume(ConsumeContext<SeatStatusUpdatedEvent> context)
        {
            var msg = context.Message;
            
            await _notifier.BroadcastSeatUpdateAsync(new {
                seatId = msg.SeatId,
                eventId = msg.EventId,
                status = msg.Status.ToString(), 
                userId = msg.UserId,
                timestamp = msg.Timestamp
            });
            
            System.Console.WriteLine($"[Notifications] Broadcasted seat update for Seat {msg.SeatId} to All. Status: {msg.Status}");
        }
    }
}
