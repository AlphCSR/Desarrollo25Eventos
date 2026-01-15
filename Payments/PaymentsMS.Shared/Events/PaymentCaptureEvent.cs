using System;

namespace PaymentsMS.Shared.Events
{
    public class PaymentCapturedEvent
    {
        public Guid BookingId { get; set; }
        public Guid TransactionId { get; set; } 
        public DateTime ProcessedAt { get; set; }
    }
}