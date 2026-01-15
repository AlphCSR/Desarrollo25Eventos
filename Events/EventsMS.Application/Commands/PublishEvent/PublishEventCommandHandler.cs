using MediatR;
using EventsMS.Domain.Interfaces;
using EventsMS.Shared.Events;
using System.Threading;
using System.Threading.Tasks;
using System;
using MassTransit;

namespace EventsMS.Application.Commands.PublishEvent
{
    public class PublishEventCommandHandler : IRequestHandler<PublishEventCommand, bool>
    {
        private readonly IEventRepository _repository;
        private readonly IPublishEndpoint _publishEndpoint;

        public PublishEventCommandHandler(IEventRepository repository, IPublishEndpoint publishEndpoint)
        {
            _repository = repository;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<bool> Handle(PublishEventCommand eventRequest, CancellationToken cancellationToken)
        {
            var eventEntity = await _repository.GetByIdAsync(eventRequest.Id, cancellationToken);
            if (eventEntity == null) return false;

            try 
            {
                eventEntity.Publish();
                await _repository.UpdateAsync(eventEntity, cancellationToken);
                await _repository.SaveChangesAsync(cancellationToken);

                await _publishEndpoint.Publish(new EventStatusChangedEvent
                {
                    EventId = eventEntity.Id,
                    NewStatus = eventEntity.Status,
                    Sections = eventEntity.Sections.Select(section => new SectionDto(section.Id, section.Name, section.Price, section.Capacity, section.IsNumbered)).ToList()
                }, cancellationToken);

                return true;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }
    }
}
