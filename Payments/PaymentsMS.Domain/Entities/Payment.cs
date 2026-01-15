using System;
using PaymentsMS.Domain.ValueObjects;

namespace PaymentsMS.Domain.Entities
{
    public class Payment
    {
        public Guid Id { get; private set; }
        public Guid BookingId { get; private set; }
        public Guid UserId { get; private set; }
        public Money Amount { get; private set; }
        public Email Email { get; private set; }
        public string Status { get; private set; }
        public string? StripePaymentIntentId { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        protected Payment() 
        { 
            Amount = null!;
            Email = null!;
            Status = "Pending";
        }

        public Payment(Guid bookingId, Guid userId, decimal amount, string currency, string email)
        {
            Id = Guid.NewGuid();
            BookingId = bookingId;
            UserId = userId;
            Amount = Money.Create(amount, currency);
            Email = Email.Create(email);
            Status = "Pending";
            CreatedAt = DateTime.UtcNow;
        }

        public void SetStripePaymentIntentId(string paymentIntentId)
        {
            StripePaymentIntentId = paymentIntentId;
        }

        public void MarkAsSucceeded()
        {
            Status = "Succeeded";
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsFailed()
        {
            Status = "Failed";
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsRefunded()
        {
            Status = "Refunded";
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
