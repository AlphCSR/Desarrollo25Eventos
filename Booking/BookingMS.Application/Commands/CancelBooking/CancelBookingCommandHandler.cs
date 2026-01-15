using MediatR;
using BookingMS.Domain.Interfaces;
using BookingMS.Shared.Events;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace BookingMS.Application.Commands.CancelBooking;

public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand, bool>
{
    private readonly IBookingRepository _repository;
    private readonly IEventPublisher _publisher;

    public CancelBookingCommandHandler(IBookingRepository repository, IEventPublisher publisher)
    {
        _repository = repository;
        _publisher = publisher;
    }

    public async Task<bool> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await _repository.GetByIdAsync(request.BookingId, cancellationToken);
        if (booking == null) return false;

        booking.Cancel(request.Reason);
        
        await _repository.UpdateAsync(booking);
        await _repository.SaveChangesAsync(cancellationToken);

        var cancelEvent = new BookingCancelledEvent
        {
            BookingId = booking.Id,
            UserId = booking.UserId,
            SeatIds = booking.SeatIds.ToList(),
            Reason = request.Reason,
            Email = booking.Email,
            Language = request.Language,
            EventName = "Evento Cancelado"
        };

        await _publisher.PublishAsync(cancelEvent, cancellationToken);

        return true;
    }
}
