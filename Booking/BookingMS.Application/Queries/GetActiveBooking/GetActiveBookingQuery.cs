using MediatR;
using BookingMS.Shared.Dtos.Response;
using System;

namespace BookingMS.Application.Queries.GetActiveBooking
{
    public record GetActiveBookingQuery(Guid UserId, Guid EventId) : IRequest<BookingDto?>;
}
