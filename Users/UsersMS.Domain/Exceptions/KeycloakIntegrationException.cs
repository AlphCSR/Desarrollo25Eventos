using System;

namespace UsersMS.Domain.Exceptions
{
    /// <summary>
    /// Excepción lanzada cuando ocurre un error en la integración con Keycloak.
    /// </summary>
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
