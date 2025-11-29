using System;

namespace EventsMS.Domain.Exceptions
{
    public class EventNotFoundException : Exception
    {
        public EventNotFoundException(Guid eventId) 
            : base($"Event with ID {eventId} was not found.")
        {
        }
    }
}
