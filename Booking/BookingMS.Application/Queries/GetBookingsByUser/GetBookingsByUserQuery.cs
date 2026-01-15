using MediatR;
using BookingMS.Shared.Dtos.Response;
using System;
using System.Collections.Generic;

namespace BookingMS.Application.Queries.GetBookingsByUser
{
    public record GetBookingsByUserQuery(Guid UserId, int Page, int PageSize) : IRequest<Shared.Dtos.PagedResult<BookingDto>>;
}
