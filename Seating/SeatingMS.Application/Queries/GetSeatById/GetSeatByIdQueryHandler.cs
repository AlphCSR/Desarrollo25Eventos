using MediatR;
using SeatingMS.Application.DTOs;
using SeatingMS.Domain.Interfaces;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SeatingMS.Application.Queries.GetSeatById
{
    public class GetSeatByIdQueryHandler : IRequestHandler<GetSeatByIdQuery, SeatDto>
    {
        private readonly IEventSeatRepository _repository;

        public GetSeatByIdQueryHandler(IEventSeatRepository repository)
        {
            _repository = repository;
        }

        public async Task<SeatDto> Handle(GetSeatByIdQuery request, CancellationToken cancellationToken)
        {
            var seat = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (seat == null) return null;

            return new SeatDto(
                seat.Id,
                seat.Row,
                seat.Number,
                seat.Status.ToString(),
                null,
                seat.CurrentUserId,
                seat.SectionId
            );
        }
    }
}
