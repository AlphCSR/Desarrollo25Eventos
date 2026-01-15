using MassTransit;
using BookingMS.Domain.Interfaces;
using BookingMS.Shared.Events;
using EventsMS.Shared.Events;
using System.Threading.Tasks;
using System.Linq;

namespace BookingMS.Infrastructure.Consumers;

public class EventCancelledConsumer : IConsumer<EventCancelledEvent>
{
    private readonly IBookingRepository _repository;
    private readonly IEventPublisher _publisher;

    public EventCancelledConsumer(IBookingRepository repository, IEventPublisher publisher)
    {
        _repository = repository;
        _publisher = publisher;
    }

    public async Task Consume(ConsumeContext<EventCancelledEvent> context)
    {
        var eventId = context.Message.EventId;
        var reason = $"Evento Cancelado: {context.Message.Reason}";

        var bookings = await _repository.GetByEventIdAsync(eventId);

        foreach (var booking in bookings)
        {
            if (booking.Status != BookingMS.Shared.Enums.BookingStatus.Cancelled)
            {
                booking.Cancel(reason);
                await _repository.UpdateAsync(booking);

                await _publisher.PublishAsync(new BookingCancelledEvent
                {
                    BookingId = booking.Id,
                    UserId = booking.UserId,
                    SeatIds = booking.SeatIds.ToList(),
                    Reason = reason,
                    Email = booking.Email
                }, context.CancellationToken);
            }
        }

        await _repository.SaveChangesAsync(context.CancellationToken);
    }
}
