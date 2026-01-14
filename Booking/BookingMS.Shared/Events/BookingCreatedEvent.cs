using System;

namespace BookingMS.Shared.Events;

public class BookingCreatedEvent
{
    public Guid BookingId { get; set; }
    public Guid UserId { get; set; }
    public decimal TotalAmount { get; set; }
    public string Email { get; set; } = string.Empty;
}
