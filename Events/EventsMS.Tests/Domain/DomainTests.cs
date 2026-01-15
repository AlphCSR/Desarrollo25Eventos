using FluentAssertions;
using System;
using Xunit;
using EventsMS.Domain.Entities;

namespace EventsMS.Tests.Domain
{
    public class DomainTests
    {
        [Fact]
        public void EventSection_ShouldGenerateSeats_WhenNumbered()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var capacity = 5;
            
            // Act
            var section = new EventSection(eventId, "S1", 100, capacity, true);

            // Assert
            section.Seats.Should().HaveCount(capacity);
            section.Seats.Should().Contain(s => s.Row == "A1" && s.Number == 1);
            section.Seats.Should().Contain(s => s.Row == "A5" && s.Number == 5);
        }

        [Fact]
        public void EventSection_ShouldNotGenerateSeats_WhenNotNumbered()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var capacity = 5;

            // Act
            var section = new EventSection(eventId, "S1", 100, capacity, false);

            // Assert
            section.Seats.Should().BeEmpty();
        }

        [Fact]
        public void Seat_ShouldInitializeCorrectly()
        {
            // Arrange
            var sectionId = Guid.NewGuid();
            
            // Act
            var seat = new Seat(sectionId, "B", 10);

            // Assert
            seat.SectionId.Should().Be(sectionId);
            seat.Row.Should().Be("B");
            seat.Number.Should().Be(10);
            seat.Status.Should().Be(EventsMS.Shared.Enums.SeatStatus.Available);
        }

        [Fact]
        public void Seat_ShouldLock_WhenAvailable()
        {
            // Arrange
            var seat = new Seat(Guid.NewGuid(), "A", 1);
            var userId = Guid.NewGuid();

            // Act
            seat.Lock(userId);

            // Assert
            seat.Status.Should().Be(EventsMS.Shared.Enums.SeatStatus.Locked);
            seat.UserId.Should().Be(userId);
        }

        [Fact]
        public void Seat_ShouldBook_WhenLocked()
        {
            // Arrange
            var seat = new Seat(Guid.NewGuid(), "A", 1);
            seat.Lock(Guid.NewGuid());

            // Act
            seat.Book();

            // Assert
            seat.Status.Should().Be(EventsMS.Shared.Enums.SeatStatus.Booked);
        }

        [Fact]
        public void Seat_ShouldRelease_WhenLocked()
        {
            // Arrange
            var seat = new Seat(Guid.NewGuid(), "A", 1);
            seat.Lock(Guid.NewGuid());

            // Act
            seat.Release();

            // Assert
            seat.Status.Should().Be(EventsMS.Shared.Enums.SeatStatus.Available);
            seat.UserId.Should().BeNull();
        }
    }
}
