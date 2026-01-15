using MediatR;
using BookingMS.Shared.Dtos.Response;
using System;
using System.Collections.Generic;

namespace BookingMS.Application.Commands.CreateBooking
{
    public record CreateBookingCommand(Guid UserId, Guid EventId, List<Guid> SeatIds, List<Guid> ServiceIds, decimal TotalAmount, string UserEmail, string? CouponCode = null) : IRequest<BookingDto>;
}