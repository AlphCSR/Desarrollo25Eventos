using System;

namespace SeatingMS.Domain.Exceptions
{
    public class SeatNotAvailableException : Exception
    {
        public SeatNotAvailableException(string message) : base(message)
        {
        }
    }
}
