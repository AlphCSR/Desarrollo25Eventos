using MediatR;
using EventsMS.Application.DTOs;
using EventsMS.Domain.Interfaces;
using EventsMS.Domain.Entities;
using EventsMS.Shared.Events;
using MassTransit;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventsMS.Application.Commands.CreateEvent
{
    public class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, Guid>
    {
        private readonly IEventRepository _repository;
        private readonly IPublishEndpoint _publishEndpoint;

        public CreateEventCommandHandler(IEventRepository repository, IPublishEndpoint publishEndpoint)
        {
            _repository = repository;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<Guid> Handle(CreateEventCommand request, CancellationToken cancellationToken)
        {
            // 1. Crear el Root
            var newEvent = new Domain.Entities.Event(
                request.EventData.Title,
                request.EventData.Description,
                request.EventData.Date,
                request.EventData.VenueName
            );

            if(!string.IsNullOrEmpty(request.EventData.ImageUrl))
            {
                newEvent.SetImageUrl(request.EventData.ImageUrl);
            }

            // 2. Agregar secciones (El dominio se encarga de generar asientos si es numerado)
            if (request.EventData.Sections != null)
            {
                foreach (var sec in request.EventData.Sections)
                {
                    newEvent.AddSection(sec.Name, sec.Price, sec.Capacity, sec.IsNumbered);
                }
            }

            // 3. Persistir (Transaccional por defecto en EF Core)
            await _repository.AddAsync(newEvent, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            // 4. Publicar Evento
            var eventCreated = new EventCreatedEvent
            {
                EventId = newEvent.Id,
                Title = newEvent.Title,
                Sections = newEvent.Sections.Select(s => new SectionDto(s.Id, s.Name, s.Price, s.Capacity, s.IsNumbered)).ToList()
            };

            await _publishEndpoint.Publish(eventCreated, cancellationToken);

            return newEvent.Id;
        }
    }
}