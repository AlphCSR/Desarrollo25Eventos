using System;
using PaymentsMS.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace PaymentsMS.Tests.Domain
{
    public class PaymentDomainTests
    {
        [Fact]
        public void Payment_Lifecycle_Tests()
        {
            var bookingId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var amount = 150.00m;
            var currency = "USD";
            var email = "client@example.com";

            // --- 1. Construction ---
            var payment = new Payment(bookingId, userId, amount, currency, email);
            
            payment.Status.Should().Be("Pending");
            payment.Amount.Amount.Should().Be(amount);
            payment.Email.Value.Should().Be(email);
            payment.BookingId.Should().Be(bookingId);

            // --- 2. External ID ---
            payment.SetStripePaymentIntentId("pi_123");
            payment.StripePaymentIntentId.Should().Be("pi_123");

            // --- 3. Transitions ---
            payment.MarkAsSucceeded();
            payment.Status.Should().Be("Succeeded");
            payment.UpdatedAt.Should().NotBeNull();

            payment.MarkAsRefunded();
            payment.Status.Should().Be("Refunded");
            
            payment.MarkAsFailed();
            payment.Status.Should().Be("Failed");
        }
    }
}
