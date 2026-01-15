using System;
using System.Collections.Generic;
using EventsMS.Shared.Enums;

namespace EventsMS.Application.DTOs
{
    public record CreateSectionDto(string Name, decimal Price, int Capacity, bool IsNumbered);
    
    public record CreateEventDto(
        Guid IdUser,
        string Title, 
        string Description, 
        DateTime Date, 
        DateTime EndDate, 
        string VenueName, 
        List<string> Categories,
        string ImageUrl,
        EventType Type,
        string? StreamingUrl,
        List<CreateSectionDto> Sections
    );
}
