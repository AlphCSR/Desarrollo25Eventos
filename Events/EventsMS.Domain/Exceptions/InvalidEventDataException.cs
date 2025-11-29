using System;

namespace EventsMS.Domain.Exceptions
{
    public class InvalidEventDataException : Exception
    {
        public InvalidEventDataException(string message) : base(message)
        {
        }
    }
}
