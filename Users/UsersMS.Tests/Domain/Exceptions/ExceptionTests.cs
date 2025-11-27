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
            // Arrange
            var message = "Invalid data";

            // Act
            var exception = new InvalidUserDataException(message);

            // Assert
            exception.Message.Should().Be(message);
        }

        [Fact]
        public void KeycloakIntegrationException_ShouldSetMessage()
        {
            // Arrange
            var message = "Keycloak error";

            // Act
            var exception = new KeycloakIntegrationException(message);

            // Assert
            exception.Message.Should().Be(message);
        }

        [Fact]
        public void KeycloakIntegrationException_ShouldSetMessageAndInnerException()
        {
            // Arrange
            var message = "Keycloak error";
            var innerException = new Exception("Inner error");

            // Act
            var exception = new KeycloakIntegrationException(message, innerException);

            // Assert
            exception.Message.Should().Be(message);
            exception.InnerException.Should().Be(innerException);
        }
    }
}
