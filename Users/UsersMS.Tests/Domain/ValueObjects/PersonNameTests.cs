using FluentAssertions;
using UsersMS.Domain.Exceptions;
using UsersMS.Domain.ValueObjects;
using Xunit;

namespace UsersMS.Tests.Domain.ValueObjects
{
    public class PersonNameTests
    {
        [Theory]
        [InlineData("Juan Perez", true, null)]
        [InlineData("A", false, "El nombre debe tener al menos 2 caracteres.")]
        [InlineData("", false, "El nombre no puede estar vacío.")]
        [InlineData("   ", false, "El nombre no puede estar vacío.")]
        [InlineData(null, false, "El nombre no puede estar vacío.")]
        public void Create_AllCases(string? input, bool shouldSucceed, string? expectedErrorMessage)
        {
            if (shouldSucceed)
            {
                var result = PersonName.Create(input!);
                result.Should().NotBeNull();
                result.Value.Should().Be(input);
            }
            else
            {
                Action act = () => PersonName.Create(input!);
                act.Should().Throw<InvalidUserDataException>()
                   .WithMessage(expectedErrorMessage);
            }
        }
    }
}
