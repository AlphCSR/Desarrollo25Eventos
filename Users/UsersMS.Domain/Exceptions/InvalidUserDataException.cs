using System;

namespace UsersMS.Domain.Exceptions
{

    public class InvalidUserDataException : DomainException
    {
        public InvalidUserDataException(string message) : base(message)
        {
        }
    }
}
