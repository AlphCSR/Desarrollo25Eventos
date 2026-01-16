using System;
using ReportsMS.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace ReportsMS.Tests.Domain
{
    public class SalesRecordTests
    {
        [Fact]
        public void SalesRecord_Creation_ShouldSetPropertiesCorrectly()
        {
            var eventId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var email = "test@example.com";
            var amount = 150.0m;
            var date = DateTime.UtcNow;

            var record = new SalesRecord(eventId, bookingId, userId, email, amount, date);

            record.Id.Should().NotBeEmpty();
            record.EventId.Should().Be(eventId);
            record.BookingId.Should().Be(bookingId);
            record.UserId.Should().Be(userId);
            record.UserEmail.Value.Should().Be(email);
            record.Amount.Amount.Should().Be(amount);
            record.Date.Should().Be(date);
        }
    }
}
