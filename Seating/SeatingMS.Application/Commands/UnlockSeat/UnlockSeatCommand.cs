using MediatR;
using System;

namespace SeatingMS.Application.Commands.UnlockSeat
{
    public class UnlockSeatCommand : IRequest<bool>
    {
        public Guid SeatId { get; }
        public Guid UserId { get; }

        public UnlockSeatCommand(Guid seatId, Guid userId)
        {
            SeatId = seatId;
            UserId = userId;
        }
    }
}
