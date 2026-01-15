using System;

namespace ServicesMS.Shared.Events
{
    public class ServiceBookedEvent
    {
        public Guid ServiceBookingId { get; set; }
        public Guid ServiceDefinitionId { get; set; }
        public Guid UserId { get; set; }
        public Guid BookingId { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
