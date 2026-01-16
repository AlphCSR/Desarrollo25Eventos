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
        public void Booking_Lifecycle_Tests()
        {
            var userId = Guid.NewGuid();
            var eventId = Guid.NewGuid();
            var seatId = Guid.NewGuid();
            var seats = new List<Guid> { seatId };
            var email = "test@example.com";

            // --- 1. Construction ---
            var booking = new Booking(userId, eventId, seats, new List<Guid>(), 100.00m, email);
            
            booking.Status.Should().Be(BookingStatus.PendingPayment);
            booking.TotalAmount.Amount.Should().Be(100.00m);
            booking.SeatIds.Should().Contain(seatId);

            // --- 2. Modifications (Seats) ---
            // Success: Remove seat while pending
            booking.RemoveSeat(seatId);
            booking.SeatIds.Should().NotContain(seatId);

            // Setup for next steps
            booking = new Booking(userId, eventId, seats, new List<Guid>(), 100.00m, email);

            // --- 3. Status Transitions ---
            // Success: Confirm Payment
            booking.ConfirmPayment();
            booking.Status.Should().Be(BookingStatus.Confirmed);

            // Error: Remove seat when confirmed
            Action actRemoveSeatFail = () => booking.RemoveSeat(seatId);
            actRemoveSeatFail.Should().Throw<InvalidBookingStateException>()
                             .WithMessage("Solo se pueden remover asientos de reservas pendientes.");

            // Success: Cancel from confirmed (allowed in this implementation)
            booking.Cancel("User request");
            booking.Status.Should().Be(BookingStatus.Cancelled);
            booking.CancelledAt.Should().NotBeNull();

            // Error: Cancel already cancelled
            Action actCancelFail = () => booking.Cancel("Repeat");
            actCancelFail.Should().Throw<InvalidBookingStateException>()
                         .WithMessage("La reserva ya está cancelada.");
        }
    }
}
