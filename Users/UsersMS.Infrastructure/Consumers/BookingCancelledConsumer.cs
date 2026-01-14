using MassTransit;
using Microsoft.Extensions.Logging;
using UsersMS.Domain.Entities;
using UsersMS.Domain.Interfaces;
using BookingMS.Shared.Events;
using System.Threading.Tasks;

namespace UsersMS.Infrastructure.Consumers
{
    public class BookingCancelledConsumer : IConsumer<BookingCancelledEvent>
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<BookingCancelledConsumer> _logger;

        public BookingCancelledConsumer(IPublishEndpoint publishEndpoint, ILogger<BookingCancelledConsumer> logger)
        {
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<BookingCancelledEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation($"Procesando Cancelacion de Reserva para el Usuario: {message.UserId}");

            var details = $"{{\"accion\": \"Cancelacion de Reserva\", \"Id de Reserva\": \"{message.BookingId}\", \"razon\": \"{message.Reason ?? "No se proporciono razon"}\"}}";
            
            await _publishEndpoint.Publish(new UsersMS.Shared.Events.UserHistoryCreatedEvent(
                message.UserId,
                "Cancelacion de Reserva",
                details,
                DateTime.UtcNow
            ), context.CancellationToken);
        }
    }
}
