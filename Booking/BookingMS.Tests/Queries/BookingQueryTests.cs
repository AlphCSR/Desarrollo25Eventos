using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BookingMS.Application.Queries.GetActiveBooking;
using BookingMS.Application.Queries.GetBookingsByUser;
using BookingMS.Domain.Entities;
using BookingMS.Domain.Interfaces;
using BookingMS.Shared.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace BookingMS.Tests.Queries
{
    public class BookingQueryTests
    {
        private readonly Mock<IBookingRepository> _repositoryMock;

        public BookingQueryTests()
        {
            _repositoryMock = new Mock<IBookingRepository>();
        }

        [Fact]
        public async Task Handle_GetActiveBooking_ShouldReturnBooking_WhenExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var eventId = Guid.NewGuid();
            var booking = new Booking(userId, eventId, new List<Guid>(), 100);
            
            _repositoryMock.Setup(x => x.GetActiveByEventAsync(userId, eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);

            var handler = new GetActiveBookingQueryHandler(_repositoryMock.Object);
            var query = new GetActiveBookingQuery(userId, eventId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.UserId.Should().Be(userId);
            result.EventId.Should().Be(eventId);
            result.Status.Should().Be(BookingStatus.PendingPayment);
        }

        [Fact]
        public async Task Handle_GetActiveBooking_ShouldReturnNull_WhenNotExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var eventId = Guid.NewGuid();

            _repositoryMock.Setup(x => x.GetActiveByEventAsync(userId, eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Booking?)null);

            var handler = new GetActiveBookingQueryHandler(_repositoryMock.Object);
            var query = new GetActiveBookingQuery(userId, eventId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_GetBookingsByUser_ShouldReturnList()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var bookings = new List<Booking>
            {
                new Booking(userId, Guid.NewGuid(), new List<Guid>(), 100),
                new Booking(userId, Guid.NewGuid(), new List<Guid>(), 200)
            };

            _repositoryMock.Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(bookings);

            var handler = new GetBookingsByUserQueryHandler(_repositoryMock.Object);
            var query = new GetBookingsByUserQuery(userId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(b => b.UserId == userId);
        }
    }
}
