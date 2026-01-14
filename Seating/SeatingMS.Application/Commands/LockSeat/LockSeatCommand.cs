using MediatR;
using System;

namespace SeatingMS.Application.Commands.LockSeat
{
    public record LockSeatCommand(Guid SeatId, Guid UserId) : IRequest<bool>;
}