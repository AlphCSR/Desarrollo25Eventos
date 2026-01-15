using System;
using ServicesMS.Domain.ValueObjects;

namespace ServicesMS.Domain.Entities
{
    public class ServiceBooking
    {
        public Guid Id { get; private set; }
        public Guid ServiceDefinitionId { get; private set; }
        public Guid UserId { get; private set; }
        public Guid BookingId { get; private set; } 
        public int Quantity { get; private set; }
        public Money TotalPrice { get; private set; }
        public string Status { get; private set; }

        public ServiceBooking(Guid serviceDefinitionId, Guid userId, Guid bookingId, int quantity, decimal unitPrice)
        {
            Id = Guid.NewGuid();
            ServiceDefinitionId = serviceDefinitionId;
            UserId = userId;
            BookingId = bookingId;
            Quantity = quantity;
            TotalPrice = (Money)(quantity * unitPrice);
            Status = "Pending";
        }

        protected ServiceBooking() 
        { 
            TotalPrice = null!;
            Status = "Pending";
        }

        public void Confirm()
        {
            Status = "Confirmed";
        }

        public void Cancel()
        {
            Status = "Cancelled";
        }
    }
}
