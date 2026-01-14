using System;

namespace BookingMS.Shared.Events
{
    public class PaymentFailedEvent
    {
        public Guid BookingId { get; set; }
        public string Reason { get; set; }
    }
}