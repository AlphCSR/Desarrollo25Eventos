using MediatR;
using EventsMS.Application.DTOs;
using EventsMS.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace EventsMS.Application.Queries.GetEventById
{
    public class GetEventByIdQueryHandler : IRequestHandler<GetEventByIdQuery, EventDto?>
    {
        private readonly IEventRepository _repository;

        public GetEventByIdQueryHandler(IEventRepository repository)
        {
            _repository = repository;
        }

        public async Task<EventDto?> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
        {
            var eventEntity = await _repository.GetByIdAsync(request.Id, cancellationToken);

            if (eventEntity == null) return null;

            return new EventDto
            {
                Id = eventEntity.Id,
                Title = eventEntity.Title,
                Description = eventEntity.Description,
                Date = eventEntity.Date,
                VenueName = eventEntity.VenueName,
                ImageUrl = eventEntity.ImageUrl,
                Status = eventEntity.Status
            };
        }
    }
}
