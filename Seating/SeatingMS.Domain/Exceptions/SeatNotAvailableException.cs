using System;

namespace SeatingMS.Domain.Exceptions
{
    public class SeatNotAvailableException : DomainException
    {
        public SeatNotAvailableException(string message) : base(message)
        {
        }
    }
}
