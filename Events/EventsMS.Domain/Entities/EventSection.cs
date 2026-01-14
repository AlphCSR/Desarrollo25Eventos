using System;
using System.Collections.Generic;
using System.Linq;
using EventsMS.Shared.Enums;

namespace EventsMS.Domain.Entities
{
    public class EventSection
    {
        public Guid Id { get; private set; }
        public Guid EventId { get; private set; }
        public string Name { get; private set; } // Ejemplo: "Platea A", "VIP"
        public decimal Price { get; private set; }
        public int Capacity { get; private set; }
        public bool IsNumbered { get; private set; } // Si es 'true', generamos asientos individuales.

        // Asientos generados (si aplica)
        private readonly List<Seat> _seats = new();
        public IReadOnlyCollection<Seat> Seats => _seats.AsReadOnly();

        protected EventSection() { }

        public EventSection(Guid eventId, string name, decimal price, int capacity, bool isNumbered)
        {
            Id = Guid.NewGuid();
            EventId = eventId;
            Name = name;
            Price = price;
            Capacity = capacity;
            IsNumbered = isNumbered;

            if (isNumbered)
            {
                GenerateSeats(capacity);
            }
        }

        private void GenerateSeats(int count)
        {
            // Lógica simple: Generar asientos secuenciales. 
            // En un sistema real, esto podría venir de una plantilla de escenario compleja.
            for (int i = 1; i <= count; i++)
            {
                _seats.Add(new Seat(Id, $"A{i}", i)); // Fila A, Asiento i
            }
        }
    }
}