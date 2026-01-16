using FluentAssertions;
using UsersMS.Domain.Exceptions;
using UsersMS.Domain.ValueObjects;
using Xunit;

namespace UsersMS.Tests.Domain.ValueObjects
{
    public class PhoneNumberTests
    {
        [Theory]
        [InlineData("+58412000000", true, null)]
        [InlineData("58412000000", true, null)]
        [InlineData("", true, null)]
        [InlineData(null, true, null)]
        [InlineData("invalid", false, "El formato del número de teléfono es inválido.")]
        [InlineData("++58412000000", false, "El formato del número de teléfono es inválido.")]
        [InlineData("1", false, "El formato del número de teléfono es inválido.")]
        public void Create_AllCases(string? input, bool shouldSucceed, string? expectedErrorMessage)
        {
            if (shouldSucceed)
            {
                var result = PhoneNumber.Create(input);
                if (string.IsNullOrWhiteSpace(input))
                {
                    result.Should().BeNull();
                }
                else
                {
                    result.Should().NotBeNull();
                    result?.Value.Should().Be(input);
                }
            }
            else
            {
                Action act = () => PhoneNumber.Create(input);
                act.Should().Throw<InvalidUserDataException>()
                   .WithMessage(expectedErrorMessage);
            }
        }
    }
}
