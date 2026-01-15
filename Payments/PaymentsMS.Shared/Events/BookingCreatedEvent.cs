using System;

namespace PaymentsMS.Shared.Events
{
    public class BookingCreatedEvent
    {
        public Guid BookingId { get; set; }
        public Guid UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Email { get; set; }
        public string PaymentMethodId { get; set; } 
    }
}