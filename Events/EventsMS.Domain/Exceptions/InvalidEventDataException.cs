using System;

namespace EventsMS.Domain.Exceptions
{
    public class InvalidEventDataException : DomainException
    {
        public InvalidEventDataException(string message) : base(message)
        {
        }
    }
}
