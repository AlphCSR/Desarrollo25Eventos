using FluentAssertions;
using MarketingMS.Domain.Exceptions;
using MarketingMS.Domain.ValueObjects;
using Xunit;
using System;

namespace MarketingMS.Tests.Domain.ValueObjects
{
    public class CouponCodeTests
    {
        [Theory]
        [InlineData("SAVE10", "SAVE10")]
        [InlineData("  save20  ", "SAVE20")]
        public void Create_ValidCode_ShouldNormalize(string input, string expected)
        {
            var result = CouponCode.Create(input);
            result.Value.Should().Be(expected);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Create_InvalidCode_ShouldThrowException(string input)
        {
            Action act = () => CouponCode.Create(input);
            act.Should().Throw<InvalidMarketingDataException>()
               .WithMessage("El código de cupón no puede estar vacío.");
        }
    }
}
