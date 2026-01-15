using MassTransit;
using Microsoft.Extensions.Logging;
using UsersMS.Domain.Entities;
using UsersMS.Domain.Interfaces;
using BookingMS.Shared.Events;
using System.Threading.Tasks;

namespace UsersMS.Infrastructure.Consumers
{
    public class BookingCreatedConsumer : IConsumer<BookingCreatedEvent>
    {
        private readonly IUserRepository _repository;
        private readonly ILogger<BookingCreatedConsumer> _logger;

        public BookingCreatedConsumer(IUserRepository repository, ILogger<BookingCreatedConsumer> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<BookingCreatedEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation($"Procesando Creacion de Reserva para el Usuario: {message.UserId}");

            var details = $"{{\"accion\": \"Creacion de Reserva\", \"Id de Reserva\": \"{message.BookingId}\", \"monto\": {message.TotalAmount}}}";
            
            var user = await _repository.GetByIdAsync(message.UserId, context.CancellationToken);
            if (user != null)
            {
                 var history = new UserHistory(message.UserId, "Creacion de Reserva", details, DateTime.UtcNow);
                 await _repository.AddHistoryAsync(history, context.CancellationToken);
                 await _repository.SaveChangesAsync(context.CancellationToken);
            }
        }
    }
}
