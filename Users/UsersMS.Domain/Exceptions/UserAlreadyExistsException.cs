using System;

namespace UsersMS.Domain.Exceptions
{

    public class UserAlreadyExistsException : DomainException
    {
        public UserAlreadyExistsException(string message) : base(message)
        {
        }
    }
}
