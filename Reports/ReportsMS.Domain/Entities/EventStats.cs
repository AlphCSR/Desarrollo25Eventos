using System;

namespace ReportsMS.Domain.Entities
{
    public class EventStats
    {
        public Guid Id { get; private set; }
        public Guid EventId { get; private set; }
        public int TotalCapacity { get; private set; }
        public int SoldSeats { get; private set; }

        public EventStats(Guid eventId, int totalCapacity)
        {
            Id = Guid.NewGuid();
            EventId = eventId;
            TotalCapacity = totalCapacity;
            SoldSeats = 0;
        }

        public void IncrementSoldSeats(int count = 1)
        {
            SoldSeats += count;
        }
    }
}
