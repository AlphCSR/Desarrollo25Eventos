using MediatR;
using EventsMS.Application.DTOs;
using EventsMS.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EventsMS.Application.Queries.GetEvents
{
    public class GetEventsQueryHandler : IRequestHandler<GetEventsQuery, List<EventDto>>
    {
        private readonly IEventRepository _repository;

        public GetEventsQueryHandler(IEventRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<EventDto>> Handle(GetEventsQuery request, CancellationToken cancellationToken)
        {
            var events = await _repository.GetAllAsync(cancellationToken);

            return events.Select(e => new EventDto
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                Date = e.Date,
                VenueName = e.VenueName,
                ImageUrl = e.ImageUrl,
                Status = e.Status
            }).ToList();
        }
    }
}
