using System;

using System.Diagnostics.CodeAnalysis;

namespace SeatingMS.Shared.Events
{
    [ExcludeFromCodeCoverage]
    public record SeatUnlockedEvent(Guid SeatId, Guid UserId, DateTime UnlockedAt);
}
