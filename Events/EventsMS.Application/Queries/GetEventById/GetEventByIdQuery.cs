using MediatR;
using EventsMS.Application.DTOs;
using System;

namespace EventsMS.Application.Queries.GetEventById
{
    public record GetEventByIdQuery(Guid Id) : IRequest<EventDto?>;
}
