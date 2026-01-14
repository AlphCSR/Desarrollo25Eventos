using SeatingMS.Shared.Enum;
using System;
using System.ComponentModel.DataAnnotations;
using SeatingMS.Domain.Exceptions;

namespace SeatingMS.Domain.Entities
{
    public class EventSeat
    {
        public Guid Id { get; private set; }
        public Guid EventId { get; private set; }
        public Guid SectionId { get; private set; }
        public string Row { get; private set; }
        public int Number { get; private set; }
        public SeatStatus Status { get; private set; }
        public Guid? CurrentUserId { get; private set; } 
        public DateTime? LockExpirationTime { get; private set; }
        
        [ConcurrencyCheck]
        public Guid Version { get; private set; }

        protected EventSeat() 
        { 
            Row = null!;
        }

        public EventSeat(Guid eventId, Guid sectionId, string row, int number)
        {
            Id = Guid.NewGuid();
            EventId = eventId;
            SectionId = sectionId;
            Row = row;
            Number = number;
            Status = SeatStatus.Available;
            Version = Guid.NewGuid();
        }

        public void Lock(Guid userId, int durationInMinutes = 5)
        {
            if (Status != SeatStatus.Available)
                throw new SeatNotAvailableException($"El asiento {Row}-{Number} no está disponible.");

            Status = SeatStatus.Locked;
            CurrentUserId = userId;
            LockExpirationTime = DateTime.UtcNow.AddMinutes(durationInMinutes);
            Version = Guid.NewGuid(); 
        }

        public void Release()
        {
            Status = SeatStatus.Available;
            CurrentUserId = null;
            LockExpirationTime = null;
            Version = Guid.NewGuid();
        }

        public void Book()
        {
            if (Status != SeatStatus.Locked && Status != SeatStatus.Available)
                throw new InvalidOperationException($"No se puede reservar el asiento {Row}-{Number} porque su estado es {Status}.");
            
            Status = SeatStatus.Booked;
            LockExpirationTime = null;
            Version = Guid.NewGuid();
        }
    }
}