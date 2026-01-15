using MediatR;
using BookingMS.Domain.Interfaces;
using BookingMS.Shared.Dtos.Response;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BookingMS.Application.Queries.GetActiveBooking
{
    public class GetActiveBookingQueryHandler : IRequestHandler<GetActiveBookingQuery, BookingDto?>
    {
        private readonly IBookingRepository _repository;

        public GetActiveBookingQueryHandler(IBookingRepository repository)
        {
            _repository = repository;
        }

        public async Task<BookingDto?> Handle(GetActiveBookingQuery request, CancellationToken cancellationToken)
        {
            var booking = await _repository.GetActiveByEventAsync(request.UserId, request.EventId, cancellationToken);

            if (booking == null) return null;

            return new BookingDto(
                booking.Id,
                booking.UserId,
                booking.EventId,
                booking.SeatIds.ToList(),
                booking.ServiceIds.ToList(),
                booking.TotalAmount,
                booking.Status,
                booking.CreatedAt,
                booking.CouponCode,
                booking.DiscountAmount
            );
        }
    }
}
