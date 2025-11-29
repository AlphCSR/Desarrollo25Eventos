using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BookingMS.Application.Commands.CreateBooking;
using BookingMS.Application.Commands.PayBooking;
using BookingMS.Domain.Entities;
using BookingMS.Domain.Exceptions;
using BookingMS.Domain.Interfaces;
using BookingMS.Shared.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace BookingMS.Tests.Handlers
{
    public class BookingHandlerTests
    {
        private readonly Mock<IBookingRepository> _repositoryMock;
        private readonly Mock<IEventPublisher> _publisherMock;

        public BookingHandlerTests()
        {
            _repositoryMock = new Mock<IBookingRepository>();
            _publisherMock = new Mock<IEventPublisher>();
        }

        public static IEnumerable<object[]> CreateBookingScenarios =>
            new List<object[]>
            {
                new object[] { Guid.NewGuid(), Guid.NewGuid(), new List<Guid> { Guid.NewGuid() }, 100m }
            };

        [Theory]
        [MemberData(nameof(CreateBookingScenarios))]
        public async Task Handle_CreateBooking_ShouldSucceed(Guid userId, Guid eventId, List<Guid> seatIds, decimal totalAmount)
        {
            // Arrange
            var command = new CreateBookingCommand(userId, eventId, seatIds, totalAmount);
            var handler = new CreateBookingCommandHandler(_repositoryMock.Object, _publisherMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(BookingStatus.PendingPayment);
            result.UserId.Should().Be(userId);
            result.EventId.Should().Be(eventId);
            result.TotalAmount.Should().Be(totalAmount);

            _repositoryMock.Verify(x => x.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Once);
            _publisherMock.Verify(x => x.PublishAsync(It.IsAny<BookingMS.Shared.Events.BookingCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_PayBooking_Success()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var booking = new Booking(Guid.NewGuid(), Guid.NewGuid(), new List<Guid>(), 100);
            typeof(Booking).GetProperty("Id")?.SetValue(booking, bookingId);

            _repositoryMock.Setup(x => x.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);

            var handler = new PayBookingCommandHandler(_repositoryMock.Object);
            var command = new PayBookingCommand(bookingId);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            booking.Status.Should().Be(BookingStatus.Confirmed);
            _repositoryMock.Verify(x => x.UpdateAsync(booking), Times.Once);
        }

        [Fact]
        public async Task Handle_PayBooking_NotFound_ShouldThrowException()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            _repositoryMock.Setup(x => x.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Booking?)null);

            var handler = new PayBookingCommandHandler(_repositoryMock.Object);
            var command = new PayBookingCommand(bookingId);

            // Act & Assert
            await Assert.ThrowsAsync<BookingNotFoundException>(() => 
                handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_PayBooking_AlreadyConfirmed_ShouldReturnTrue()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var booking = new Booking(Guid.NewGuid(), Guid.NewGuid(), new List<Guid>(), 100);
            typeof(Booking).GetProperty("Id")?.SetValue(booking, bookingId);
            booking.ConfirmPayment(); // Already confirmed

            _repositoryMock.Setup(x => x.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);

            var handler = new PayBookingCommandHandler(_repositoryMock.Object);
            var command = new PayBookingCommand(bookingId);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            _repositoryMock.Verify(x => x.UpdateAsync(booking), Times.Never);
        }
    }
}
