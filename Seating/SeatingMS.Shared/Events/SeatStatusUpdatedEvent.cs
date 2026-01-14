using System;
using SeatingMS.Shared.Enum;

namespace SeatingMS.Shared.Events
{
    public record SeatStatusUpdatedEvent
    {
        public Guid SeatId { get; init; }
        public Guid EventId { get; init; }
        public SeatStatus Status { get; init; }
        public Guid? UserId { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    }
}
