using MediatR;
using EventsMS.Domain.Interfaces;
using EventsMS.Shared.Events;
using MassTransit;
using System.Threading;
using System.Threading.Tasks;

namespace EventsMS.Application.Commands.CancelEvent;

public class CancelEventCommandHandler : IRequestHandler<CancelEventCommand, bool>
{
    private readonly IEventRepository _repository;
    private readonly IPublishEndpoint _publishEndpoint;

    public CancelEventCommandHandler(IEventRepository repository, IPublishEndpoint publishEndpoint)
    {
        _repository = repository;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<bool> Handle(CancelEventCommand request, CancellationToken cancellationToken)
    {
        var eventCancel = await _repository.GetByIdAsync(request.EventId, cancellationToken);
        if (eventCancel == null) return false;

        if (!request.IsAdmin && eventCancel.IdUser != request.UserId)
        {
            throw new UnauthorizedAccessException("No tienes permisos para cancelar este evento.");
        }

        eventCancel.Cancel();
        
        await _repository.UpdateAsync(eventCancel, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        var cancelEvent = new EventCancelledEvent
        {
            EventId = eventCancel.Id,
            Title = eventCancel.Title,
            Reason = request.Reason
        };

        await _publishEndpoint.Publish(cancelEvent, cancellationToken);

        return true;
    }
}
