using MediatR;
using BookingMS.Domain.Interfaces;
using BookingMS.Shared.Dtos.Response;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BookingMS.Application.Queries.GetBookingsByUser
{
    public class GetBookingsByUserQueryHandler : IRequestHandler<GetBookingsByUserQuery, List<BookingDto>>
    {
        private readonly IBookingRepository _repository;

        public GetBookingsByUserQueryHandler(IBookingRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<BookingDto>> Handle(GetBookingsByUserQuery request, CancellationToken cancellationToken)
        {
            var bookings = await _repository.GetByUserIdAsync(request.UserId);
            
            return bookings.Select(b => new BookingDto(
                b.Id,
                b.UserId,
                b.EventId,
                b.SeatIds.ToList(),
                b.TotalAmount,
                b.Status,
                b.CreatedAt
            )).ToList();
        }
    }
}
