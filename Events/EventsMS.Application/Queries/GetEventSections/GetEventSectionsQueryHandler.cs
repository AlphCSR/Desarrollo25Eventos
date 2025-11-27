using MediatR;
using EventsMS.Application.DTOs;
using EventsMS.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EventsMS.Application.Queries.GetEventSections
{
    public class GetEventSectionsQueryHandler : IRequestHandler<GetEventSectionsQuery, IEnumerable<EventSectionDto>>
    {
        private readonly IEventRepository _repository;

        public GetEventSectionsQueryHandler(IEventRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<EventSectionDto>> Handle(GetEventSectionsQuery request, CancellationToken cancellationToken)
        {
            var eventEntity = await _repository.GetByIdAsync(request.EventId, cancellationToken);

            if (eventEntity == null) return new List<EventSectionDto>();

            return eventEntity.Sections.Select(s => new EventSectionDto(
                s.Id,
                s.Name,
                s.Price,
                s.Capacity,
                s.IsNumbered
            ));
        }
    }
}
