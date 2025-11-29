using System;

namespace BookingMS.Domain.Exceptions
{
    public class InvalidBookingStateException : Exception
    {
        public InvalidBookingStateException(string message) : base(message)
        {
        }
    }
}
