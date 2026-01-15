using MediatR;
using BookingMS.Domain.Interfaces;
using BookingMS.Shared.Dtos.Response;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BookingMS.Application.Queries.GetBookingsByUser
{
    public class GetBookingsByUserQueryHandler : IRequestHandler<GetBookingsByUserQuery, Shared.Dtos.PagedResult<BookingDto>>
    {
        private readonly IBookingRepository _repository;

        public GetBookingsByUserQueryHandler(IBookingRepository repository)
        {
            _repository = repository;
        }

        public async Task<Shared.Dtos.PagedResult<BookingDto>> Handle(GetBookingsByUserQuery request, CancellationToken cancellationToken)
        {
            var (bookings, totalCount) = await _repository.GetPagedByUserIdAsync(request.UserId, request.Page, request.PageSize);
            
            var items = bookings.Select(b => new BookingDto(
                b.Id,
                b.UserId,
                b.EventId,
                b.SeatIds.ToList(),
                b.ServiceIds.ToList(),
                b.TotalAmount,
                b.Status,
                b.CreatedAt,
                b.CouponCode,
                b.DiscountAmount
            )).ToList();

            return new Shared.Dtos.PagedResult<BookingDto>(items, totalCount, request.Page, request.PageSize);
        }
    }
}
