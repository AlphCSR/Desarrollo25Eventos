using FluentAssertions;
using BookingMS.Domain.Exceptions;
using BookingMS.Domain.ValueObjects;
using Xunit;
using System;

namespace BookingMS.Tests.Domain.ValueObjects
{
    public class EmailTests
    {
        [Theory]
        [InlineData("test@example.com", true, null)]
        [InlineData("user.name@domain.co", true, null)]
        [InlineData("", false, "El email no puede estar vacío.")]
        [InlineData("   ", false, "El email no puede estar vacío.")]
        [InlineData("invalid-email", false, "El formato del email es inválido.")]
        public void Create_AllCases(string emailAddress, bool shouldSucceed, string expectedErrorMessage)
        {
            if (shouldSucceed)
            {
                var result = Email.Create(emailAddress);
                result.Should().NotBeNull();
                result.Value.Should().Be(emailAddress);
            }
            else
            {
                Action act = () => Email.Create(emailAddress);
                act.Should().Throw<InvalidBookingStateException>()
                   .WithMessage(expectedErrorMessage);
            }
        }
    }
}
