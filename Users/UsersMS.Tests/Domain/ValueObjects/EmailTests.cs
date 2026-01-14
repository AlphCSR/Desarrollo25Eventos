using FluentAssertions;
using UsersMS.Domain.Exceptions;
using UsersMS.Domain.ValueObjects;
using Xunit;

namespace UsersMS.Tests.Domain.ValueObjects
{
    public class EmailTests
    {
        [Theory]
        [InlineData("test@example.com")]
        [InlineData("user.name@domain.co.uk")]
        [InlineData("user+tag@domain.com")]
        public void Create_ShouldReturnEmail_WhenEmailIsValid(string emailAddress)
        {
            // Act
            var email = Email.Create(emailAddress);

            // Assert
            email.Should().NotBeNull();
            email.Value.Should().Be(emailAddress);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Create_ShouldThrowInvalidUserDataException_WhenEmailIsNullOrEmpty(string? emailAddress)
        {
            // Act
            Action act = () => Email.Create(emailAddress!);

            // Assert
            act.Should().Throw<InvalidUserDataException>()
                .WithMessage("El email no puede estar vacío.");
        }

        [Theory]
        [InlineData("invalid-email")]
        [InlineData("user@")]
        [InlineData("@domain.com")]
        [InlineData("user@domain")]
        public void Create_ShouldThrowInvalidUserDataException_WhenEmailFormatIsInvalid(string emailAddress)
        {
            // Act
            Action act = () => Email.Create(emailAddress);

            // Assert
            act.Should().Throw<InvalidUserDataException>()
                .WithMessage("El formato del email es inválido.");
        }

        [Fact]
        public void ImplicitOperator_ShouldReturnStringValue()
        {
            // Arrange
            var emailAddress = "test@example.com";
            var email = Email.Create(emailAddress);

            // Act
            string result = email;

            // Assert
            result.Should().Be(emailAddress);
        }

        [Fact]
        public void ExplicitOperator_ShouldReturnEmailInstance()
        {
            // Arrange
            var emailAddress = "test@example.com";

            // Act
            var email = (Email)emailAddress;

            // Assert
            email.Should().NotBeNull();
            email.Value.Should().Be(emailAddress);
        }
    }
}
