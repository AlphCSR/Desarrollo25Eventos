using MediatR;
using System;

namespace EventsMS.Application.Commands.DeleteEvent
{
    public record DeleteEventCommand(Guid Id) : IRequest;
}
