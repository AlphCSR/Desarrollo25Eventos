using MassTransit;
using Microsoft.Extensions.Logging;
using UsersMS.Domain.Entities;
using UsersMS.Domain.Interfaces;
using ServicesMS.Shared.Events;
using System.Threading.Tasks;

namespace UsersMS.Infrastructure.Consumers
{
    public class ServiceBookedConsumer : IConsumer<ServiceBookedEvent>
    {
        private readonly IUserRepository _repository;
        private readonly ILogger<ServiceBookedConsumer> _logger;

        public ServiceBookedConsumer(IUserRepository repository, ILogger<ServiceBookedConsumer> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ServiceBookedEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation($"Procesando Reserva de Servicio para el Usuario: {message.UserId}");

            var details = $"{{\"accion\": \"Reserva de Servicio\", \"Id de Servicio\": \"{message.ServiceDefinitionId}\", \"cantidad\": {message.Quantity}, \"precio\": {message.TotalPrice}}}";
            
            var user = await _repository.GetByIdAsync(message.UserId, context.CancellationToken);
            if (user != null)
            {
                 var history = new UserHistory(message.UserId, "Reserva de Servicio", details, DateTime.UtcNow);
                 await _repository.AddHistoryAsync(history, context.CancellationToken);
                 await _repository.SaveChangesAsync(context.CancellationToken);
            }
        }
    }
}
