using MediatR;
using SeatingMS.Application.DTOs;
using System;

namespace SeatingMS.Application.Queries.GetSeatById
{
    public record GetSeatByIdQuery(Guid Id) : IRequest<SeatDto>;
}
