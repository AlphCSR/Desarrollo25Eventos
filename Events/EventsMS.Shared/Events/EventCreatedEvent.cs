using System;
using System.Collections.Generic;

namespace EventsMS.Shared.Events
{
    public record SectionDto(Guid SectionId, string Name, decimal Price, int Capacity, bool IsNumbered);

    public class EventCreatedEvent
    {
        public Guid EventId { get; set; }
        public string Title { get; set; }
        public List<SectionDto> Sections { get; set; } = new();
    }
}