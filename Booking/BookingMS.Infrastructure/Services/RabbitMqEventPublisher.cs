using MassTransit;
using BookingMS.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

using System.Diagnostics.CodeAnalysis;

namespace BookingMS.Infrastructure.Services
{
    [ExcludeFromCodeCoverage]
    public class RabbitMqEventPublisher : IEventPublisher
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public RabbitMqEventPublisher(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken) where T : class
        {
            await _publishEndpoint.Publish(@event, cancellationToken);
        }
    }
}