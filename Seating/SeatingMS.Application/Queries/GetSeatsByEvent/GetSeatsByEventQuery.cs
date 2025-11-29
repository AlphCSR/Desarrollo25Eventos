using MediatR;
using SeatingMS.Application.DTOs;
using System;
using System.Collections.Generic;

namespace SeatingMS.Application.Queries.GetSeatsByEvent
{
    public record GetSeatsByEventQuery(Guid EventId) : IRequest<IEnumerable<SeatDto>>;
}
