using System;
using MarketingMS.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace MarketingMS.Tests.Domain
{
    public class CouponDomainTests
    {
        [Fact]
        public void Coupon_Validation_Tests()
        {
            var futureDate = DateTime.UtcNow.AddDays(10);
            var pastDate = DateTime.UtcNow.AddDays(-1);

            // --- 1. Basic Validity ---
            var coupon = new Coupon("SAVE50", DiscountType.FixedAmount, 50, futureDate, 10, 100);
            
            coupon.IsValid(150).Should().BeTrue("Amount 150 > Minimum 100");
            coupon.IsValid(50).Should().BeFalse("Amount 50 < Minimum 100");

            // --- 2. Expiry ---
            var expiredCoupon = new Coupon("OLD", DiscountType.Percentage, 10, pastDate);
            expiredCoupon.IsValid(100).Should().BeFalse("Coupon is expired");

            // --- 3. Usage Limit ---
            var limitedCoupon = new Coupon("LIMIT", DiscountType.Percentage, 10, futureDate, 1);
            limitedCoupon.IsValid(100).Should().BeTrue();
            limitedCoupon.IncrementUsage();
            limitedCoupon.IsValid(100).Should().BeFalse("Limit reached");

            // --- 4. Deactivation ---
            coupon.Deactivate();
            coupon.IsValid(150).Should().BeFalse("Coupon is deactivated");
        }
    }
}
