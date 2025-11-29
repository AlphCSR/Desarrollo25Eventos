using SeatingMS.Shared.Enum;
using System;

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
        public Guid? CurrentUserId { get; private set; } // Usuario que tiene el bloqueo
        public DateTime? LockExpirationTime { get; private set; }

        protected EventSeat() { }

        public EventSeat(Guid eventId, Guid sectionId, string row, int number)
        {
            Id = Guid.NewGuid();
            EventId = eventId;
            SectionId = sectionId;
            Row = row;
            Number = number;
            Status = SeatStatus.Available;
        }

        public void Lock(Guid userId, int durationInMinutes = 15)
        {
            if (Status != SeatStatus.Available)
                throw new Exceptions.SeatNotAvailableException($"El asiento {Row}-{Number} no está disponible.");

            Status = SeatStatus.Locked;
            CurrentUserId = userId;
            LockExpirationTime = DateTime.UtcNow.AddMinutes(durationInMinutes);
        }

        public void Release()
        {
            Status = SeatStatus.Available;
            CurrentUserId = null;
            LockExpirationTime = null;
        }

        public void Book()
        {
            if (Status != SeatStatus.Locked)
                throw new InvalidOperationException("No se puede reservar un asiento que no está bloqueado previamente.");
            
            Status = SeatStatus.Booked;
            LockExpirationTime = null;
        }
    }
}