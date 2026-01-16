using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Moq;
using BookingMS.Application.Queries.GetBookingById;
using BookingMS.Domain.Interfaces;
using BookingMS.Domain.Entities;
using BookingMS.Shared.Enums;

namespace BookingMS.Tests.Queries
{
    public class GetBookingByIdQueryHandlerTests
    {
        private readonly Mock<IBookingRepository> _repoMock;
        private readonly GetBookingByIdQueryHandler _handler;

        public GetBookingByIdQueryHandlerTests()
        {
            _repoMock = new Mock<IBookingRepository>();
            _handler = new GetBookingByIdQueryHandler(_repoMock.Object);
        }

        [Fact]
        public async Task Handle_GetBookingById_Success()
        {
            var bookingId = Guid.NewGuid();
            var booking = new Booking(Guid.NewGuid(), Guid.NewGuid(), new List<Guid>(), new List<Guid>(), 100, "test@test.com");
            typeof(Booking).GetProperty("Id")?.SetValue(booking, bookingId);

            _repoMock.Setup(r => r.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);

            var query = new GetBookingByIdQuery(bookingId);
            var result = await _handler.Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(bookingId);
            result.Status.Should().Be(BookingStatus.PendingPayment);
        }

        [Fact]
        public async Task Handle_GetBookingById_NotFound_ShouldReturnNull()
        {
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Booking?)null);

            var query = new GetBookingByIdQuery(Guid.NewGuid());
            var result = await _handler.Handle(query, CancellationToken.None);

            result.Should().BeNull();
        }
    }
}
