using MassTransit;
using Microsoft.Extensions.Logging;
using UsersMS.Shared.Events;
using BookingMS.Shared.Events;
using System.Threading.Tasks;

namespace UsersMS.Infrastructure.Consumers
{
    public class BookingConfirmedConsumer : IConsumer<BookingConfirmedEvent>
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<BookingConfirmedConsumer> _logger;

        public BookingConfirmedConsumer(IPublishEndpoint publishEndpoint, ILogger<BookingConfirmedConsumer> logger)
        {
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<BookingConfirmedEvent> context)
        {
            var message = context.Message;
            Console.WriteLine($"[USERS-MS] Procesando Confirmacion de Reserva para el Usuario {message.UserId} Reserva {message.BookingId}");
            _logger.LogInformation($"Procesando Confirmacion de Reserva para el Usuario: {message.UserId}");

            var details = $"{{\"accion\": \"Confirmacion de Reserva\", \"Id de Reserva\": \"{message.BookingId}\", \"Id del Evento\": \"{message.EventId}\", \"monto\": {message.TotalAmount}}}";
            
            var friendlyMessage = $"Confirmacion exitosa: {message.EventName} por ${message.TotalAmount:F2}";
            
            await _publishEndpoint.Publish(new UserHistoryCreatedEvent(
                message.UserId,
                "Confirmacion de Reserva",
                details,
                message.ConfirmedAt,
                friendlyMessage
            ), context.CancellationToken);
        }
    }
}
