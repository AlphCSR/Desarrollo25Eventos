using MediatR;
using System;

namespace BookingMS.Application.Commands.PayBooking
{
    public record PayBookingCommand(Guid BookingId) : IRequest<bool>;
}
