using MassTransit;
using Microsoft.Extensions.Logging;
using EventsMS.Shared.Events;
using UsersMS.Shared.Events;
using System.Threading.Tasks;
using System;

namespace UsersMS.Infrastructure.Consumers
{
    public class EventCreatedConsumer : IConsumer<EventCreatedEvent>
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<EventCreatedConsumer> _logger;

        public EventCreatedConsumer(IPublishEndpoint publishEndpoint, ILogger<EventCreatedConsumer> logger)
        {
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<EventCreatedEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation($"Evento creado por el Organizador: {message.IdUser}: {message.Title}");

            await _publishEndpoint.Publish(new UserHistoryCreatedEvent(
                message.IdUser,
                "Creacion de Evento",
                $"Evento '{message.Title}' creado para el dia {message.Date}",
                DateTime.UtcNow
            ), context.CancellationToken);
        }
    }
}
