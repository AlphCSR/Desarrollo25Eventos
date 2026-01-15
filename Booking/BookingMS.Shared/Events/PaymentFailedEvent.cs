using System;

namespace BookingMS.Shared.Events
{
    public class PaymentFailedEvent
    {
        public Guid BookingId { get; set; }
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Reason { get; set; }
    }
}