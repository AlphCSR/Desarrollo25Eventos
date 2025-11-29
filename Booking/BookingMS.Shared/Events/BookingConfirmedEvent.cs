using System;

namespace BookingMS.Shared.Events;

public class BookingConfirmedEvent
{
    public Guid BookingId { get; set; }
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
}
