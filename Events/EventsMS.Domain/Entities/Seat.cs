using System;
using EventsMS.Shared.Enums;

namespace EventsMS.Domain.Entities
{
    public class Seat
    {
        public Guid Id { get; private set; }
        public Guid SectionId { get; private set; }
        public string Row { get; private set; }
        public int Number { get; private set; }
        public string Code => $"{Row}-{Number}";
        public SeatStatus Status { get; private set; }
        public Guid? UserId { get; private set; }

        protected Seat() { }

        public Seat(Guid sectionId, string row, int number)
        {
            Id = Guid.NewGuid();
            SectionId = sectionId;
            Row = row;
            Number = number;
            Status = SeatStatus.Available;
        }


        public void Lock(Guid userId)
        {
            if (Status != SeatStatus.Available) throw new InvalidOperationException("El asiento no está disponible.");
            Status = SeatStatus.Locked;
            UserId = userId;
        }

        public void Release()
        {
            Status = SeatStatus.Available;
            UserId = null;
        }

        public void Book()
        {
            if (Status != SeatStatus.Locked) throw new InvalidOperationException("Solo se pueden reservar asientos previamente bloqueados.");
            Status = SeatStatus.Booked;
        }
    }
}