using System;
using ReportsMS.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace ReportsMS.Tests.Domain
{
    public class EventStatsTests
    {
        [Fact]
        public void EventStats_Creation_ShouldSetProperties()
        {
            var eventId = Guid.NewGuid();
            var capacity = 1000;
            var stats = new EventStats(eventId, capacity);

            stats.EventId.Should().Be(eventId);
            stats.TotalCapacity.Should().Be(capacity);
            stats.SoldSeats.Should().Be(0);
        }

        [Fact]
        public void IncrementSoldSeats_ShouldIncreaseCount()
        {
            var stats = new EventStats(Guid.NewGuid(), 100);
            stats.IncrementSoldSeats(5);

            stats.SoldSeats.Should().Be(5);
        }
    }
}
