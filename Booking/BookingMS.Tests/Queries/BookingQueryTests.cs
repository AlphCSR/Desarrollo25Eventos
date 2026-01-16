using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BookingMS.Application.Queries.GetActiveBooking;
using BookingMS.Application.Queries.GetBookingsByUser;
using BookingMS.Shared.Dtos.Response;
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
            
            var userId = Guid.NewGuid();
            var eventId = Guid.NewGuid();
            var booking = new Booking(userId, eventId, new List<Guid>(), new List<Guid>(), 100, "test@example.com");
            
            _repositoryMock.Setup(x => x.GetActiveByEventAsync(userId, eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);

            var handler = new GetActiveBookingQueryHandler(_repositoryMock.Object);
            var query = new GetActiveBookingQuery(userId, eventId);

            
            var result = await handler.Handle(query, CancellationToken.None);

            
            result.Should().NotBeNull();
            result!.UserId.Should().Be(userId);
            result.EventId.Should().Be(eventId);
            result.Status.Should().Be(BookingStatus.PendingPayment);
        }

        [Fact]
        public async Task Handle_GetActiveBooking_ShouldReturnNull_WhenNotExists()
        {
            
            var userId = Guid.NewGuid();
            var eventId = Guid.NewGuid();

            _repositoryMock.Setup(x => x.GetActiveByEventAsync(userId, eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Booking?)null);

            var handler = new GetActiveBookingQueryHandler(_repositoryMock.Object);
            var query = new GetActiveBookingQuery(userId, eventId);

            
            var result = await handler.Handle(query, CancellationToken.None);

            
            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_GetBookingsByUser_ShouldReturnList()
        {
            
            var userId = Guid.NewGuid();
            var bookings = new List<Booking>
            {
                new Booking(userId, Guid.NewGuid(), new List<Guid>(), new List<Guid>(), 100, "test@example.com"),
                new Booking(userId, Guid.NewGuid(), new List<Guid>(), new List<Guid>(), 200, "test2@example.com")
            };

            _repositoryMock.Setup(x => x.GetPagedByUserIdAsync(userId, 1, 10))
                .ReturnsAsync((bookings, bookings.Count));

            var handler = new GetBookingsByUserQueryHandler(_repositoryMock.Object);
            var query = new GetBookingsByUserQuery(userId, 1, 10);

            
            var result = await handler.Handle(query, CancellationToken.None);

            
            result.Items.Should().HaveCount(2);
            result.Items.Should().AllSatisfy(b => b.UserId.Should().Be(userId));
        }
    }
}
