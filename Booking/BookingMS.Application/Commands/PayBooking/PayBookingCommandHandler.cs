using MediatR;
using BookingMS.Domain.Interfaces;
using BookingMS.Domain.Entities;
using BookingMS.Shared.Enums;
using BookingMS.Domain.Exceptions;
using System.Threading;
using System.Threading.Tasks;

namespace BookingMS.Application.Commands.PayBooking
{
    public class PayBookingCommandHandler : IRequestHandler<PayBookingCommand, bool>
    {
        private readonly IBookingRepository _repository;

        public PayBookingCommandHandler(IBookingRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(PayBookingCommand request, CancellationToken cancellationToken)
        {
            var booking = await _repository.GetByIdAsync(request.BookingId, cancellationToken);
            if (booking == null) 
                throw new BookingNotFoundException(request.BookingId);

            if (booking.Status == BookingStatus.Confirmed) return true;

            // Simular pago exitoso
            booking.ConfirmPayment();

            await _repository.UpdateAsync(booking);
            await _repository.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
