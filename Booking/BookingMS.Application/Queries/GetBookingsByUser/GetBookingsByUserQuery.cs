using MediatR;
using BookingMS.Shared.Dtos.Response;
using System;
using System.Collections.Generic;

namespace BookingMS.Application.Queries.GetBookingsByUser
{
    public record GetBookingsByUserQuery(Guid UserId) : IRequest<List<BookingDto>>;
}
