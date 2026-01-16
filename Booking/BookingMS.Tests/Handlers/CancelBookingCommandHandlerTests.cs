using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Moq;
using BookingMS.Application.Commands.CancelBooking;
using BookingMS.Domain.Interfaces;
using BookingMS.Domain.Entities;
using BookingMS.Application.Interfaces;
using BookingMS.Shared.Events;
using BookingMS.Shared.Enums;

namespace BookingMS.Tests.Handlers
{
    public class CancelBookingCommandHandlerTests
    {
        private readonly Mock<IBookingRepository> _repoMock;
        private readonly Mock<IEventPublisher> _publisherMock;
        private readonly CancelBookingCommandHandler _handler;

        public CancelBookingCommandHandlerTests()
        {
            _repoMock = new Mock<IBookingRepository>();
            _publisherMock = new Mock<IEventPublisher>();
            _handler = new CancelBookingCommandHandler(_repoMock.Object, _publisherMock.Object);
        }

        [Fact]
        public async Task Handle_CancelBooking_Success()
        {
            var bookingId = Guid.NewGuid();
            var booking = new Booking(Guid.NewGuid(), Guid.NewGuid(), new List<Guid>(), new List<Guid>(), 100, "test@test.com");
            typeof(Booking).GetProperty("Id")?.SetValue(booking, bookingId);

            _repoMock.Setup(r => r.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);

            var command = new CancelBookingCommand(bookingId, "Change of plans", "es");
            var result = await _handler.Handle(command, CancellationToken.None);

            result.Should().BeTrue();
            booking.Status.Should().Be(BookingStatus.Cancelled);
            _repoMock.Verify(r => r.UpdateAsync(booking), Times.Once);
            _publisherMock.Verify(p => p.PublishAsync(It.IsAny<BookingCancelledEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_CancelBooking_NotFound_ShouldReturnFalse()
        {
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Booking?)null);

            var command = new CancelBookingCommand(Guid.NewGuid(), "Reason");
            var result = await _handler.Handle(command, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
