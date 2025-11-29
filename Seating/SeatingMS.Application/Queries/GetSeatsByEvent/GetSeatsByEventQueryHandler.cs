using MediatR;
using SeatingMS.Application.DTOs;
using SeatingMS.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SeatingMS.Application.Queries.GetSeatsByEvent
{
    public class GetSeatsByEventQueryHandler : IRequestHandler<GetSeatsByEventQuery, IEnumerable<SeatDto>>
    {
        private readonly IEventSeatRepository _repository;

        public GetSeatsByEventQueryHandler(IEventSeatRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<SeatDto>> Handle(GetSeatsByEventQuery request, CancellationToken cancellationToken)
        {
            var seats = await _repository.GetByEventIdAsync(request.EventId, cancellationToken);

            return seats.Select(s => new SeatDto(
                s.Id,
                s.Row,
                s.Number,
                s.Status.ToString(),
                null, 
                s.CurrentUserId,
                s.SectionId
            ));
        }
    }
}
