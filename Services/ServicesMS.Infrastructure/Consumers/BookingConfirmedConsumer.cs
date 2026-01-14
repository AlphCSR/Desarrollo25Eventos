using MassTransit;
using BookingMS.Shared.Events;
using ServicesMS.Domain.Interfaces;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ServicesMS.Infrastructure.Consumers
{
    public class BookingConfirmedConsumer : IConsumer<BookingConfirmedEvent>
    {
        private readonly IServiceRepository _repository;
        private readonly ILogger<BookingConfirmedConsumer> _logger;

        public BookingConfirmedConsumer(IServiceRepository repository, ILogger<BookingConfirmedConsumer> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<BookingConfirmedEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation($"Confirmando servicios para Booking {message.BookingId}");

            var bookings = await _repository.GetBookingsByBookingIdAsync(message.BookingId, context.CancellationToken);
            
            foreach (var booking in bookings)
            {
                booking.Confirm();
                await _repository.UpdateBookingAsync(booking, context.CancellationToken);
            }

            if (bookings.Any())
            {
                await _repository.SaveChangesAsync(context.CancellationToken);
                _logger.LogInformation($"{bookings.Count()} servicios confirmados para la reserva {message.BookingId}");
            }
        }
    }
}
