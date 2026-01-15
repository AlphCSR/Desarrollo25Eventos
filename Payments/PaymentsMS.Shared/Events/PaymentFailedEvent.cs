using System;

namespace PaymentsMS.Shared.Events
{
    public class PaymentFailedEvent
    {
        public Guid BookingId { get; set; }
        public string Reason { get; set; }
    }
}