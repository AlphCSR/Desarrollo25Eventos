using MassTransit;
using Microsoft.Extensions.Logging;
using EventsMS.Shared.Events;
using UsersMS.Shared.Events;
using System.Threading.Tasks;

namespace UsersMS.Infrastructure.Consumers
{
    public class EventUpdatedConsumer : IConsumer<EventUpdatedEvent>
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<EventUpdatedConsumer> _logger;

        public EventUpdatedConsumer(IPublishEndpoint publishEndpoint, ILogger<EventUpdatedConsumer> logger)
        {
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<EventUpdatedEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation($"Evento actualizado por el Organizador: {message.IdUser}: {message.Title}");

            await _publishEndpoint.Publish(new UserHistoryCreatedEvent(
                message.IdUser,
                "Actualizacion de Evento",
                $"Evento actualizado '{message.Title}': {message.Changes}",
                message.UpdatedAt
            ), context.CancellationToken);
        }
    }
}
