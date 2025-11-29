using System;

namespace EventsMS.Application.DTOs
{
    public record UpdateEventDto(string Title, string Description, DateTime Date, string VenueName, string Category);
}
