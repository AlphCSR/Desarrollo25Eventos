using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using MassTransit;
using Microsoft.Extensions.Logging;
using SeatingMS.Infrastructure.Consumers;
using SeatingMS.Domain.Interfaces;
using SeatingMS.Domain.Entities;
using BookingMS.Shared.Events;
using SeatingMS.Shared.Enum;
using SeatingMS.Shared.Events;

namespace SeatingMS.Tests.Consumers
{
    public class BookingConfirmedConsumerTests
    {
        private readonly Mock<IEventSeatRepository> _repoMock;
        private readonly Mock<ILogger<BookingConfirmedConsumer>> _loggerMock;
        private readonly BookingConfirmedConsumer _consumer;
        private readonly Mock<ConsumeContext<BookingConfirmedEvent>> _contextMock;

        public BookingConfirmedConsumerTests()
        {
            _repoMock = new Mock<IEventSeatRepository>();
            _loggerMock = new Mock<ILogger<BookingConfirmedConsumer>>();
            _consumer = new BookingConfirmedConsumer(_repoMock.Object, _loggerMock.Object);
            _contextMock = new Mock<ConsumeContext<BookingConfirmedEvent>>();
        }

        [Fact]
        public async Task Consume_ShouldBookSeats_WhenSeatsExist()
        {
            var bookingId = Guid.NewGuid();
            var seatId1 = Guid.NewGuid();
            var seatId2 = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var message = new BookingConfirmedEvent
            {
                BookingId = bookingId,
                SeatIds = new List<Guid> { seatId1, seatId2 },
                Email = "test@test.com",
                UserId = userId
            };

            _contextMock.Setup(c => c.Message).Returns(message);
            _contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

            var seat1 = new EventSeat(Guid.NewGuid(), Guid.NewGuid(), "A", 1);
            seat1.Lock(userId);
            typeof(EventSeat).GetProperty("Id")?.SetValue(seat1, seatId1);

            var seat2 = new EventSeat(Guid.NewGuid(), Guid.NewGuid(), "A", 2);
            seat2.Lock(userId);
            typeof(EventSeat).GetProperty("Id")?.SetValue(seat2, seatId2);

            _repoMock.Setup(r => r.GetByIdAsync(seatId1, It.IsAny<CancellationToken>())).ReturnsAsync(seat1);
            _repoMock.Setup(r => r.GetByIdAsync(seatId2, It.IsAny<CancellationToken>())).ReturnsAsync(seat2);

            await _consumer.Consume(_contextMock.Object);

            Assert.Equal(SeatStatus.Booked, seat1.Status);
            Assert.Equal(SeatStatus.Booked, seat2.Status);
            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<EventSeat>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _contextMock.Verify(c => c.Publish(It.IsAny<SeatStatusUpdatedEvent>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Fact]
        public async Task Consume_ShouldFailGracefully_WhenSeatNotFound()
        {
            var bookingId = Guid.NewGuid();
            var seatId = Guid.NewGuid();
            var message = new BookingConfirmedEvent { BookingId = bookingId, SeatIds = new List<Guid> { seatId } };

            _contextMock.Setup(c => c.Message).Returns(message);
            _repoMock.Setup(r => r.GetByIdAsync(seatId, It.IsAny<CancellationToken>())).ReturnsAsync((EventSeat?)null);

            await _consumer.Consume(_contextMock.Object);

            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<EventSeat>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
