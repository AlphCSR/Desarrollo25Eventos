using System;

namespace UsersMS.Domain.Exceptions
{
    /// <summary>
    /// Excepción lanzada cuando se intenta crear un usuario que ya existe.
    /// </summary>
    public class UserAlreadyExistsException : Exception
    {
        public UserAlreadyExistsException(string message) : base(message)
        {
        }
    }
}
