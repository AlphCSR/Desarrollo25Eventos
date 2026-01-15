using MediatR;
using BookingMS.Domain.Interfaces;
using BookingMS.Shared.Dtos.Response;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BookingMS.Application.Queries.GetBookingById;

public class GetBookingByIdQueryHandler : IRequestHandler<GetBookingByIdQuery, BookingDto?>
{
    private readonly IBookingRepository _repository;

    public GetBookingByIdQueryHandler(IBookingRepository repository)
    {
        _repository = repository;
    }

    public async Task<BookingDto?> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken)
    {
        var b = await _repository.GetByIdAsync(request.Id, cancellationToken);
        
        if (b == null) return null;

        return new BookingDto(
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
        );
    }
}
