using System;
using SurveyMS.Domain.ValueObjects;

namespace SurveyMS.Domain.Entities
{
    public class Feedback
    {
        public Guid Id { get; private set; }
        public Guid EventId { get; private set; }
        public Guid UserId { get; private set; }
        public Guid BookingId { get; private set; }
        public string UserName { get; private set; }
        public Rating Rating { get; private set; } 
        public string Comment { get; private set; }
        public DateTime CreatedAt { get; private set; }

        protected Feedback() 
        { 
            UserName = null!;
            Rating = null!;
            Comment = null!;
        }

        public Feedback(Guid eventId, Guid userId, Guid bookingId, string userName, int rating, string comment)
        {
            Id = Guid.NewGuid();
            EventId = eventId;
            UserId = userId;
            BookingId = bookingId;
            UserName = userName;
            Rating = Rating.Create(rating);
            Comment = comment;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
