using MediatR;
using EventsMS.Domain.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace EventsMS.Application.Commands.DeleteEvent
{
    public class DeleteEventCommandHandler : IRequestHandler<DeleteEventCommand>
    {
        private readonly IEventRepository _repository;

        public DeleteEventCommandHandler(IEventRepository repository)
        {
            _repository = repository;
        }

        public async Task Handle(DeleteEventCommand request, CancellationToken cancellationToken)
        {
            var eventEntity = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (eventEntity == null)
            {
                throw new KeyNotFoundException($"Event with ID {request.Id} not found.");
            }

            eventEntity.Cancel(); // Soft delete as per plan

            await _repository.UpdateAsync(eventEntity, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
        }
    }
}
