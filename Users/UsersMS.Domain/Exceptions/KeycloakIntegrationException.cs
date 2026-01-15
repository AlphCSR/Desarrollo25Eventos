using System;

namespace UsersMS.Domain.Exceptions
{

    public class KeycloakIntegrationException : Exception
    {
        public KeycloakIntegrationException(string message) : base(message)
        {
        }

        public KeycloakIntegrationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
