using MediatR;
using EventsMS.Domain.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace EventsMS.Application.Commands.UpdateEvent
{
    public class UpdateEventCommandHandler : IRequestHandler<UpdateEventCommand>
    {
        private readonly IEventRepository _repository;

        public UpdateEventCommandHandler(IEventRepository repository)
        {
            _repository = repository;
        }

        public async Task Handle(UpdateEventCommand request, CancellationToken cancellationToken)
        {
            var eventEntity = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (eventEntity == null)
            {
                throw new KeyNotFoundException($"Event with ID {request.Id} not found.");
            }

            eventEntity.UpdateDetails(request.Title, request.Description, request.Date, request.VenueName, request.Category);

            await _repository.UpdateAsync(eventEntity, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
        }
    }
}
