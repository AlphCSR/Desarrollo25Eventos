using System;
using FluentAssertions;
using SeatingMS.Domain.Entities;
using SeatingMS.Domain.Exceptions;
using SeatingMS.Shared.Enum;
using Xunit;

namespace SeatingMS.Tests.Domain
{
    public class EventSeatTests
    {
        [Fact]
        public void Lock_ShouldSetStatusToLocked_WhenAvailable()
        {
            // Arrange
            var seat = new EventSeat(Guid.NewGuid(), Guid.NewGuid(), "A", 1);
            var userId = Guid.NewGuid();

            // Act
            seat.Lock(userId);

            // Assert
            seat.Status.Should().Be(SeatStatus.Locked);
            seat.CurrentUserId.Should().Be(userId);
        }

        [Fact]
        public void Lock_ShouldThrowException_WhenAlreadyLocked()
        {
            // Arrange
            var seat = new EventSeat(Guid.NewGuid(), Guid.NewGuid(), "A", 1);
            var userId1 = Guid.NewGuid();
            var userId2 = Guid.NewGuid();
            seat.Lock(userId1);

            // Act
            Action act = () => seat.Lock(userId2);

            // Assert
            act.Should().Throw<SeatNotAvailableException>();
        }

        [Fact]
        public void Release_ShouldSetStatusToAvailable()
        {
            // Arrange
            var seat = new EventSeat(Guid.NewGuid(), Guid.NewGuid(), "A", 1);
            seat.Lock(Guid.NewGuid());

            // Act
            seat.Release();

            // Assert
            seat.Status.Should().Be(SeatStatus.Available);
            seat.CurrentUserId.Should().BeNull();
        }

        [Fact]
        public void Book_ShouldSetStatusToBooked()
        {
            // Arrange
            var seat = new EventSeat(Guid.NewGuid(), Guid.NewGuid(), "A", 1);
            seat.Lock(Guid.NewGuid());

            // Act
            seat.Book();

            // Assert
            seat.Status.Should().Be(SeatStatus.Booked);
        }
    }
}
