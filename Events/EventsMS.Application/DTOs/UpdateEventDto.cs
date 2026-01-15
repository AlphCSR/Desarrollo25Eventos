using System;
using System.Collections.Generic;

namespace EventsMS.Application.DTOs
{
    public record UpdateEventDto(
        string Title, 
        string Description, 
        DateTime Date, 
        DateTime EndDate, 
        string VenueName, 
        List<string> Categories,
        EventsMS.Shared.Enums.EventType Type,
        string? StreamingUrl
    );
}
