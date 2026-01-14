using BookingMS.Shared.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using SeatingMS.Domain.Interfaces;
using SeatingMS.Shared.Enum;
using System.Threading.Tasks;

namespace SeatingMS.Infrastructure.Consumers
{
    public class BookingConfirmedConsumer : IConsumer<BookingConfirmedEvent>
    {
        private readonly IEventSeatRepository _repository;
        private readonly ILogger<BookingConfirmedConsumer> _logger;

        public BookingConfirmedConsumer(IEventSeatRepository repository, ILogger<BookingConfirmedConsumer> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<BookingConfirmedEvent> context)
        {
            var bookingEvent = context.Message;
            _logger.LogInformation("Procesando BookingConfirmedEvent para BookingId: {BookingId}. Actualizando {Count} asientos.", 
                bookingEvent.BookingId, bookingEvent.SeatIds.Count);

            foreach (var seatId in bookingEvent.SeatIds)
            {
                var seat = await _repository.GetByIdAsync(seatId, context.CancellationToken);
                if (seat != null)
                {
                    seat.Book();
                    await _repository.UpdateAsync(seat, context.CancellationToken);
                    _logger.LogInformation("Asiento {SeatId} actualizado a Booked.", seatId);

                    await context.Publish(new SeatingMS.Shared.Events.SeatStatusUpdatedEvent
                    {
                        SeatId = seat.Id,
                        EventId = seat.EventId,
                        Status = seat.Status,
                        UserId = seat.CurrentUserId
                    });
                }
                else
                {
                    _logger.LogWarning("Asiento {SeatId} no encontrado para la reserva {BookingId}.", seatId, bookingEvent.BookingId);
                }
            }

            await _repository.SaveChangesAsync(context.CancellationToken);
            _logger.LogInformation("Se actualizaron exitosamente todos los asientos para la reserva {BookingId}", bookingEvent.BookingId);
        }
    }
}
