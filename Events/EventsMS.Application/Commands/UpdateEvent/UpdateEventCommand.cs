using MediatR;
using System;

namespace EventsMS.Application.Commands.UpdateEvent
{
    public record UpdateEventCommand(Guid Id, string Title, string Description, DateTime Date, string VenueName, string Category) : IRequest;
}
