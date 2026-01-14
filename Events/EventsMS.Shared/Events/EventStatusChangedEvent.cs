using System;
using EventsMS.Shared.Enums;

namespace EventsMS.Shared.Events
{
    public record EventStatusChangedEvent
    {
        public Guid EventId { get; init; }
        public EventStatus NewStatus { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public List<SectionDto>? Sections { get; init; } 
    }
}
