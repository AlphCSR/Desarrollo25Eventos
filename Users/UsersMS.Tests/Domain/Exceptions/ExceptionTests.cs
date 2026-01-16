using FluentAssertions;
using System;
using UsersMS.Domain.Exceptions;
using Xunit;

namespace UsersMS.Tests.Domain.Exceptions
{
    public class ExceptionTests
    {
        [Fact]
        public void InvalidUserDataException_ShouldSetMessage()
        {
            
            var message = "Invalid data";

            
            var exception = new InvalidUserDataException(message);

            
            exception.Message.Should().Be(message);
        }

        [Fact]
        public void KeycloakIntegrationException_ShouldSetMessage()
        {
            
            var message = "Keycloak error";

            
            var exception = new KeycloakIntegrationException(message);

            
            exception.Message.Should().Be(message);
        }

        [Fact]
        public void KeycloakIntegrationException_ShouldSetMessageAndInnerException()
        {
            
            var message = "Keycloak error";
            var innerException = new Exception("Inner error");

            
            var exception = new KeycloakIntegrationException(message, innerException);

            
            exception.Message.Should().Be(message);
            exception.InnerException.Should().Be(innerException);
        }
    }
}
