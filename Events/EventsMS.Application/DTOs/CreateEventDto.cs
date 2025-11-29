using System;
using System.Collections.Generic;

namespace EventsMS.Application.DTOs
{
    public record CreateSectionDto(string Name, decimal Price, int Capacity, bool IsNumbered);
    
    public record CreateEventDto(
        string Title, 
        string Description, 
        DateTime Date, 
        string VenueName, 
        string Category,
        string ImageUrl,
        List<CreateSectionDto> Sections
    );
}
