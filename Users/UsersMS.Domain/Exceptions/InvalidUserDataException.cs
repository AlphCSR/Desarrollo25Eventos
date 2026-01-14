using System;

namespace UsersMS.Domain.Exceptions
{
    /// <summary>
    /// Excepción lanzada cuando los datos del usuario no son válidos.
    /// </summary>
    public class InvalidUserDataException : Exception
    {
        public InvalidUserDataException(string message) : base(message)
        {
        }
    }
}
