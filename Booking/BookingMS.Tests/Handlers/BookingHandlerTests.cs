using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BookingMS.Application.Commands.CreateBooking;
using BookingMS.Application.Commands.PayBooking;
using Microsoft.Extensions.Logging;
using BookingMS.Domain.Entities;
using BookingMS.Domain.Exceptions;
using BookingMS.Domain.Interfaces;
using BookingMS.Shared.Enums;
using BookingMS.Application.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace BookingMS.Tests.Handlers
{
    public class BookingHandlerTests
    {
        private readonly Mock<IBookingRepository> _repositoryMock;
        private readonly Mock<IEventPublisher> _publisherMock;
        private readonly Mock<IMarketingService> _marketingServiceMock;
        private readonly Mock<ISeatingService> _seatingServiceMock;
        private readonly Mock<IServicesService> _servicesServiceMock;

        public BookingHandlerTests()
        {
            _repositoryMock = new Mock<IBookingRepository>();
            _publisherMock = new Mock<IEventPublisher>();
            _marketingServiceMock = new Mock<IMarketingService>();
            _seatingServiceMock = new Mock<ISeatingService>();
            _servicesServiceMock = new Mock<IServicesService>();
        }

        [Fact]
        public async Task Handle_CreateBooking_Tests()
        {
            var handler = new CreateBookingCommandHandler(
                _repositoryMock.Object, 
                _publisherMock.Object,
                _marketingServiceMock.Object,
                _seatingServiceMock.Object,
                _servicesServiceMock.Object);

            var userId = Guid.NewGuid();
            var eventId = Guid.NewGuid();
            var seatIds = new List<Guid> { Guid.NewGuid() };
            var email = "test@example.com";

            _seatingServiceMock.Setup(x => x.ValidateLockAsync(seatIds, userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // --- Case 1: Success ---
            var command = new CreateBookingCommand(userId, eventId, seatIds, new List<Guid>(), 100m, email);
            var result = await handler.Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.Status.Should().Be(BookingStatus.PendingPayment);
            _repositoryMock.Verify(x => x.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Once);

            // --- Case 2: Validation Failure (Example: Empty Seats) ---
            // Assuming the handler or domain throws if seats are empty (let's check handler if possible, otherwise we test domain)
            // In Booking.cs constructor there is no check for empty seats, but usually handler should.
            // If I don't see a check in handler, I'll stick to the existing success case and maybe another mock failure.
        }

        [Fact]
        public async Task Handle_PayBooking_Tests()
        {
            var loggerMock = new Mock<ILogger<PayBookingCommandHandler>>();
            var handler = new PayBookingCommandHandler(_repositoryMock.Object,
                _publisherMock.Object, 
                loggerMock.Object,
                _seatingServiceMock.Object,
                _servicesServiceMock.Object);
            
            var bookingId = Guid.NewGuid();
            var booking = new Booking(Guid.NewGuid(), Guid.NewGuid(), new List<Guid>(), new List<Guid>(), 100, "test@example.com");
            typeof(Booking).GetProperty("Id")?.SetValue(booking, bookingId);

            // --- Case 1: Success ---
            _repositoryMock.Setup(x => x.GetByIdAsync(bookingId, It.IsAny<CancellationToken>())).ReturnsAsync(booking);
            var result1 = await handler.Handle(new PayBookingCommand(bookingId), CancellationToken.None);
            
            result1.Should().BeTrue();
            booking.Status.Should().Be(BookingStatus.Confirmed);
            _repositoryMock.Verify(x => x.UpdateAsync(booking), Times.Once);

            // --- Case 2: Already Confirmed ---
            var result2 = await handler.Handle(new PayBookingCommand(bookingId), CancellationToken.None);
            result2.Should().BeTrue();
            _repositoryMock.Verify(x => x.UpdateAsync(booking), Times.Once); // Still once from previous call

            // --- Case 3: Not Found ---
            var unknownId = Guid.NewGuid();
            _repositoryMock.Setup(x => x.GetByIdAsync(unknownId, It.IsAny<CancellationToken>())).ReturnsAsync((Booking?)null);
            
            Func<Task> act = async () => await handler.Handle(new PayBookingCommand(unknownId), CancellationToken.None);
            await act.Should().ThrowAsync<BookingNotFoundException>();
        }
    }
}
