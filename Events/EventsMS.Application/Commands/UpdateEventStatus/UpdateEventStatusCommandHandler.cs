using MediatR;
using EventsMS.Domain.Interfaces;
using EventsMS.Shared.Events;
using MassTransit;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace EventsMS.Application.Commands.UpdateEventStatus
{
    public class UpdateEventStatusCommandHandler : IRequestHandler<UpdateEventStatusCommand, bool>
    {
        private readonly IEventRepository _repository;
        private readonly IPublishEndpoint _publishEndpoint;

        public UpdateEventStatusCommandHandler(IEventRepository repository, IPublishEndpoint publishEndpoint)
        {
            _repository = repository;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<bool> Handle(UpdateEventStatusCommand request, CancellationToken cancellationToken)
        {
            var eventEntity = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (eventEntity == null) return false;

            eventEntity.UpdateStatus(request.NewStatus);
            await _repository.UpdateAsync(eventEntity, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            await _publishEndpoint.Publish(new EventStatusChangedEvent
            {
                EventId = eventEntity.Id,
                NewStatus = eventEntity.Status,
                Sections = eventEntity.Sections.Select(s => new SectionDto(s.Id, s.Name, s.Price, s.Capacity, s.IsNumbered)).ToList()
            }, cancellationToken);

            return true;
        }
    }
}
