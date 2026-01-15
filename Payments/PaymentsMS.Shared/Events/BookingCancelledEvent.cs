using System;
using System.Collections.Generic;

namespace PaymentsMS.Shared.Events;

public record BookingCancelledEvent
{
    public Guid BookingId { get; init; }
    public Guid UserId { get; init; }
    public List<Guid> SeatIds { get; init; } = new();
    public string Reason { get; init; } = string.Empty;
}
