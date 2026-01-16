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
        public void EventSeat_Lifecycle_Tests()
        {
            var eventId = Guid.NewGuid();
            var sectionId = Guid.NewGuid();
            var row = "A";
            var number = 10;
            var userId = Guid.NewGuid();

            var seat = new EventSeat(eventId, sectionId, row, number);
            
            seat.Status.Should().Be(SeatStatus.Available);
            seat.Row.Should().Be(row);
            seat.Number.Should().Be(number);

            seat.Lock(userId, 10);
            seat.Status.Should().Be(SeatStatus.Locked);
            seat.CurrentUserId.Should().Be(userId);
            seat.LockExpirationTime.Should().BeAfter(DateTime.UtcNow);

            var otherUser = Guid.NewGuid();
            Action actLockFail = () => seat.Lock(otherUser);
            actLockFail.Should().Throw<SeatNotAvailableException>();

            seat.Book();
            seat.Status.Should().Be(SeatStatus.Booked);
            seat.LockExpirationTime.Should().BeNull();

            Action actBookFail = () => seat.Book();
            actBookFail.Should().Throw<InvalidOperationException>()
                       .WithMessage("*Booked*");

            seat.Release();
            seat.Status.Should().Be(SeatStatus.Available);
            seat.CurrentUserId.Should().BeNull();
        }
    }
}
