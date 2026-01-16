using FluentAssertions;
using EventsMS.Domain.ValueObjects;
using Xunit;
using System;

namespace EventsMS.Tests.Domain.ValueObjects
{
    public class MoneyTests
    {
        [Theory]
        [InlineData(100.50, "USD", true, null)]
        [InlineData(0, "EUR", true, null)]
        [InlineData(-1, "USD", false, "El monto no puede ser negativo.")]
        [InlineData(50, "", false, "La moneda es requerida.")]
        [InlineData(50, "   ", false, "La moneda es requerida.")]
        public void Create_AllCases(decimal amount, string currency, bool shouldSucceed, string expectedErrorMessage)
        {
            if (shouldSucceed)
            {
                var result = Money.Create(amount, currency);
                result.Should().NotBeNull();
                result.Amount.Should().Be(amount);
                result.Currency.Should().Be(currency);
            }
            else
            {
                Action act = () => Money.Create(amount, currency);
                act.Should().Throw<ArgumentException>()
                   .WithMessage(expectedErrorMessage);
            }
        }

        [Fact]
        public void Operators_Tests()
        {
            var money = Money.Create(100m, "USD");
            
            decimal value = money;
            value.Should().Be(100m);

            var fromDecimal = (Money)200m;
            fromDecimal.Amount.Should().Be(200m);
            fromDecimal.Currency.Should().Be("USD");
        }
    }
}
