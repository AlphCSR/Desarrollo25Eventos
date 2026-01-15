using System;
using ReportsMS.Domain.ValueObjects;

namespace ReportsMS.Domain.Entities
{
    public class SalesRecord
    {
        public Guid Id { get; private set; }
        public Guid EventId { get; private set; }
        public Guid BookingId { get; private set; }
        public Guid UserId { get; private set; }
        public Email UserEmail { get; private set; }
        public Money Amount { get; private set; }
        public DateTime Date { get; private set; }

        protected SalesRecord() 
        { 
            UserEmail = null!;
            Amount = null!;
        }

        public SalesRecord(Guid eventId, Guid bookingId, Guid userId, string userEmail, decimal amount, DateTime date)
        {
            Id = Guid.NewGuid();
            EventId = eventId;
            BookingId = bookingId;
            UserId = userId;
            UserEmail = Email.Create(userEmail);
            Amount = (Money)amount;
            Date = date;
        }
    }
}
