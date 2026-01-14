using MediatR;
using System;
using System.Collections.Generic;
using EventsMS.Shared.Enums;

namespace EventsMS.Application.Commands.UpdateEvent
{
    public record UpdateEventCommand(
        Guid Id, 
        string Title, 
        string Description, 
        DateTime Date, 
        DateTime EndDate, 
        string VenueName, 
        List<string> Categories,
        EventType Type,
        string? StreamingUrl
    ) : IRequest;
}
