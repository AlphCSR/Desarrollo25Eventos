using FluentAssertions;
using PaymentsMS.Domain.ValueObjects;
using Xunit;
using System;

namespace PaymentsMS.Tests.Domain.ValueObjects
{
    public class MoneyTests
    {
        [Theory]
        [InlineData(100.50, "USD", true, null)]
        [InlineData(0, "USD", true, null)]
        [InlineData(-10, "USD", false, "El monto no puede ser negativo.")]
        [InlineData(50, "", false, "La moneda es requerida.")]
        public void Create_AllCases(decimal amount, string currency, bool shouldSucceed, string expectedErrorMessage)
        {
            if (shouldSucceed)
            {
                var result = Money.Create(amount, currency);
                result.Should().NotBeNull();
                result.Amount.Should().Be(amount);
            }
            else
            {
                Action act = () => Money.Create(amount, currency);
                act.Should().Throw<ArgumentException>()
                   .WithMessage(expectedErrorMessage);
            }
        }
    }
}
