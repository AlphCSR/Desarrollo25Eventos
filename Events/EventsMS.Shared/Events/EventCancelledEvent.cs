using System;

namespace EventsMS.Shared.Events
{
    public record EventCancelledEvent
    {
        public Guid EventId { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Reason { get; init; } = string.Empty;
    }
}
