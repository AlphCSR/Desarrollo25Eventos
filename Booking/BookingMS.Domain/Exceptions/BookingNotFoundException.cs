using System;

namespace BookingMS.Domain.Exceptions
{
    public class BookingNotFoundException : Exception
    {
        public BookingNotFoundException(Guid bookingId) 
            : base($"Booking with ID {bookingId} was not found.")
        {
        }
    }
}
