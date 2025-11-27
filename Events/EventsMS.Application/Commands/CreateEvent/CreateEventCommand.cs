using MediatR;
using EventsMS.Application.DTOs;
using System;

namespace EventsMS.Application.Commands.CreateEvent
{
    public record CreateEventCommand(CreateEventDto EventData) : IRequest<Guid>;
}