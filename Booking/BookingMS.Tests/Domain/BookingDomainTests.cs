using System;
using System.Collections.Generic;
using BookingMS.Domain.Entities;
using BookingMS.Domain.Exceptions;
using BookingMS.Shared.Enums;
using FluentAssertions;
using Xunit;

namespace BookingMS.Tests.Domain
{
    public class BookingDomainTests
    {
        [Fact]
        public void Cancel_ShouldSetStatusToCancelled_WhenPending()
        {
            // Arrange
            var booking = new Booking(Guid.NewGuid(), Guid.NewGuid(), new List<Guid>(), 100);

            // Act
            booking.Cancel("Test reason");

            // Assert
            booking.Status.Should().Be(BookingStatus.Cancelled);
        }

        [Fact]
        public void Cancel_ShouldThrowException_WhenConfirmed()
        {
            // Arrange
            var booking = new Booking(Guid.NewGuid(), Guid.NewGuid(), new List<Guid>(), 100);
            booking.ConfirmPayment();

            // Act
            Action act = () => booking.Cancel("Test reason");

            // Assert
            act.Should().Throw<InvalidBookingStateException>()
                .WithMessage("*ya pagada*");
        }

        [Fact]
        public void RemoveSeat_ShouldRemoveSeat_WhenPending()
        {
            // Arrange
            var seatId = Guid.NewGuid();
            var booking = new Booking(Guid.NewGuid(), Guid.NewGuid(), new List<Guid> { seatId }, 100);

            // Act
            booking.RemoveSeat(seatId);

            // Assert
            booking.SeatIds.Should().NotContain(seatId);
        }

        [Fact]
        public void RemoveSeat_ShouldThrowException_WhenConfirmed()
        {
            // Arrange
            var seatId = Guid.NewGuid();
            var booking = new Booking(Guid.NewGuid(), Guid.NewGuid(), new List<Guid> { seatId }, 100);
            booking.ConfirmPayment();

            // Act
            Action act = () => booking.RemoveSeat(seatId);

            // Assert
            act.Should().Throw<InvalidBookingStateException>()
                .WithMessage("*Solo se pueden remover asientos de reservas pendientes*");
        }
    }
}
