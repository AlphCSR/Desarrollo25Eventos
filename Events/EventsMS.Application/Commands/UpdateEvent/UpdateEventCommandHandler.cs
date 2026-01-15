using MediatR;
using EventsMS.Domain.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using MassTransit;
using EventsMS.Shared.Events;

namespace EventsMS.Application.Commands.UpdateEvent
{
    public class UpdateEventCommandHandler : IRequestHandler<UpdateEventCommand>
    {
        private readonly IEventRepository _repository;
        private readonly IPublishEndpoint _publishEndpoint;

        public UpdateEventCommandHandler(IEventRepository repository, IPublishEndpoint publishEndpoint)
        {
            _repository = repository;
            _publishEndpoint = publishEndpoint;
        }

        public async Task Handle(UpdateEventCommand request, CancellationToken cancellationToken)
        {
            var eventEntity = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (eventEntity == null)
            {
                throw new KeyNotFoundException($"Evento con ID {request.Id} no encontrado.");
            }

            var oldTitle = eventEntity.Title;
            var oldDescription = eventEntity.Description;
            var oldDate = eventEntity.Date;
            var oldEndDate = eventEntity.EndDate;
            var oldVenue = eventEntity.VenueName;
            var oldCategories = string.Join(", ", eventEntity.Categories ?? new List<string>());

            eventEntity.UpdateDetails(request.Title, request.Description, request.Date, request.EndDate, request.VenueName, request.Categories, request.Type, request.StreamingUrl);

            await _repository.UpdateAsync(eventEntity, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            var changesList = new List<string>();
            if (oldTitle != eventEntity.Title) changesList.Add($"Titulo cambiado de '{oldTitle}' a '{eventEntity.Title}'");
            if (oldDescription != eventEntity.Description) changesList.Add("Descripcion cambiada");
            if (oldDate != eventEntity.Date) changesList.Add($"Fecha cambiada de '{oldDate}' a '{eventEntity.Date}'");
            if (oldEndDate != eventEntity.EndDate) changesList.Add($"Fecha Final cambiada de '{oldEndDate}' a '{eventEntity.EndDate}'");
            if (oldVenue != eventEntity.VenueName) changesList.Add($"Venue cambiado de '{oldVenue}' a '{eventEntity.VenueName}'");
            
            var newCategories = string.Join(", ", eventEntity.Categories ?? new List<string>());
            if (oldCategories != newCategories) changesList.Add($"Categorias cambiadas de [{oldCategories}] a [{newCategories}]");

            var changesString = changesList.Count > 0 ? string.Join("; ", changesList) : "Detalles del evento actualizados";

            await _publishEndpoint.Publish(new EventUpdatedEvent
            {
                EventId = eventEntity.Id,
                IdUser = eventEntity.IdUser,
                Title = eventEntity.Title,
                Changes = changesString,
                UpdatedAt = DateTime.UtcNow
            }, cancellationToken);
        }
    }
}
