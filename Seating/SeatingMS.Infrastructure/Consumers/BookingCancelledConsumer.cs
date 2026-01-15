using MassTransit;
using MediatR;
using BookingMS.Shared.Events;
using SeatingMS.Application.Commands.UnlockSeat;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace SeatingMS.Infrastructure.Consumers
{
    public class BookingCancelledConsumer : IConsumer<BookingCancelledEvent>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<BookingCancelledConsumer> _logger;

        public BookingCancelledConsumer(IMediator mediator, ILogger<BookingCancelledConsumer> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<BookingCancelledEvent> context)
        {
            _logger.LogInformation("Procesando cancelación de reserva {BookingId}. Asientos a liberar: {SeatCount}. IDs: {SeatIds}", 
                context.Message.BookingId, 
                context.Message.SeatIds?.Count ?? 0, 
                string.Join(", ", context.Message.SeatIds ?? new List<Guid>()));
            
            if (context.Message.SeatIds == null || !context.Message.SeatIds.Any())
            {
                _logger.LogWarning("No se recibieron SeatIds para la cancelación de la reserva {BookingId}", context.Message.BookingId);
                return;
            }

            foreach (var seatId in context.Message.SeatIds)
            {
                 try 
                 {
                    _logger.LogInformation("Enviando UnlockSeatCommand para Asiento {SeatId} (Reserva {BookingId})", seatId, context.Message.BookingId);
                    await _mediator.Send(new UnlockSeatCommand(seatId, context.Message.UserId));
                 }
                 catch(System.Exception ex)
                 {
                    _logger.LogError(ex, "Error al intentar desbloquear/liberar el asiento {SeatId} durante la cancelación de la reserva {BookingId}", seatId, context.Message.BookingId);
                 }
            }
        }
    }
}
