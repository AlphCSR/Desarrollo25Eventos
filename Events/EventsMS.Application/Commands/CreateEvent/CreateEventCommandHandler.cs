using MediatR;
using EventsMS.Application.DTOs;
using EventsMS.Domain.Interfaces;
using EventsMS.Domain.Entities;
using Event = EventsMS.Domain.Entities.Event;
using EventsMS.Domain.Exceptions;
using EventsMS.Shared.Events;
using MassTransit;
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
            
            if (string.IsNullOrWhiteSpace(request.EventData.Title))
                throw new InvalidEventDataException("El título del evento no puede estar vacío.");

            if (string.IsNullOrWhiteSpace(request.EventData.Description))
                throw new InvalidEventDataException("La descripción del evento no puede estar vacía.");

            if (request.EventData.Date <= DateTime.UtcNow)
                throw new InvalidEventDataException("La fecha del evento debe ser en el futuro.");

            var newEvent = new Event(
                request.EventData.IdUser,
                request.EventData.Title,
                request.EventData.Description,
                request.EventData.Date,
                request.EventData.EndDate,
                request.EventData.VenueName,
                request.EventData.Categories,
                request.EventData.Type,
                request.EventData.StreamingUrl
            );

            if(!string.IsNullOrEmpty(request.EventData.ImageUrl))
            {
                newEvent.SetImageUrl(request.EventData.ImageUrl);
            }

            if (request.EventData.Sections != null)
            {
                foreach (var sec in request.EventData.Sections)
                {
                    newEvent.AddSection(sec.Name, sec.Price, sec.Capacity, sec.IsNumbered);
                }
            }

            await _repository.AddAsync(newEvent, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            var eventCreated = new EventCreatedEvent
            {
                EventId = newEvent.Id,
                IdUser = newEvent.IdUser,
                Title = newEvent.Title,
                Date = newEvent.Date,
                EndDate = newEvent.EndDate,
                Categories = newEvent.Categories,
                Sections = newEvent.Sections.Select(s => new SectionDto(s.Id, s.Name, s.Price, s.Capacity, s.IsNumbered)).ToList()
            };

            await _publishEndpoint.Publish(eventCreated, cancellationToken);

            return newEvent.Id;
        }
    }
}
