using System;

namespace MarketingMS.Domain.Entities
{
    public class UserInterest
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public string Category { get; private set; }
        public int Score { get; private set; }
        public DateTime LastUpdated { get; private set; }

        public UserInterest(Guid userId, string category)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            Category = category;
            Score = 1;
            LastUpdated = DateTime.UtcNow;
        }

        public void IncrementScore()
        {
            Score++;
            LastUpdated = DateTime.UtcNow;
        }

        public void SetScore(int newScore)
        {
            Score = newScore;
            LastUpdated = DateTime.UtcNow;
        }
    }
}
