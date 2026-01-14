using System;

namespace EventsMS.Application.DTOs
{
    public record EventSectionDto(Guid Id, string Name, decimal Price, int Capacity, bool IsNumbered);
}
