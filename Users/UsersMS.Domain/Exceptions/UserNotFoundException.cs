using System;

namespace UsersMS.Domain.Exceptions
{
    /// <summary>
    /// Excepción lanzada cuando se intenta acceder a un usuario que no existe.
    /// </summary>
    public class UserNotFoundException : Exception
    {
        public UserNotFoundException(string message) : base(message)
        {
        }
    }
}
