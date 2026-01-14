using System;

namespace BookingMS.Shared.Events
{
    public record PaymentExpiringSoonEvent
    {
        public Guid BookingId { get; init; }
        public Guid UserId { get; init; }
        public string Email { get; init; } = string.Empty;
        public int MinutesRemaining { get; init; }
    }
}
