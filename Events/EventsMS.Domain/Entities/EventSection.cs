using System;
using System.Collections.Generic;
using System.Linq;
using EventsMS.Domain.ValueObjects;

namespace EventsMS.Domain.Entities
{
    public class EventSection
    {
        public Guid Id { get; private set; }
        public Guid EventId { get; private set; }
        public string Name { get; private set; }
        public Money Price { get; private set; }
        public int Capacity { get; private set; }
        public bool IsNumbered { get; private set; }

        private readonly List<Seat> _seats = new();
        public IReadOnlyCollection<Seat> Seats => _seats.AsReadOnly();

        protected EventSection() 
        { 
            Name = null!;
            Price = null!;
        }

        public EventSection(Guid eventId, string name, decimal price, int capacity, bool isNumbered)
        {
            Id = Guid.NewGuid();
            EventId = eventId;
            Name = name;
            Price = (Money)price;
            Capacity = capacity;
            IsNumbered = isNumbered;

            if (isNumbered)
            {
                GenerateSeats(capacity);
            }
        }

        private void GenerateSeats(int count)
        {
            for (int i = 1; i <= count; i++)
            {
                _seats.Add(new Seat(Id, $"A{i}", i));
            }
        }
    }
}