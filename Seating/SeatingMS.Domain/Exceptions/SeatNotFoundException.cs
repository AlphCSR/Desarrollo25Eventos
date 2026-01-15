using System;

namespace SeatingMS.Domain.Exceptions
{
    public class SeatNotFoundException : DomainException
    {
        public SeatNotFoundException(Guid seatId) 
            : base($"El asiento con ID {seatId} no fue encontrado.")
        {
        }
    }
}
