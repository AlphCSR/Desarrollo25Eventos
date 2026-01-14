using System;

namespace SeatingMS.Domain.Exceptions
{
    public class SeatNotFoundException : Exception
    {
        public SeatNotFoundException(Guid seatId) 
            : base($"Seat with ID {seatId} was not found.")
        {
        }
    }
}
