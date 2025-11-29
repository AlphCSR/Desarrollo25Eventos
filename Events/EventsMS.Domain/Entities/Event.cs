using System;
using System.Collections.Generic;
using System.Linq;
using EventsMS.Shared.Enums;

namespace EventsMS.Domain.Entities
{
    public class Event 
    {
        public Guid Id { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        public DateTime Date { get; private set; }
        public string VenueName { get; private set; } // Nombre del lugar (Estadio, Teatro...)
        public string? ImageUrl { get; private set; } // URL del Blob Storage
        public string Category { get; private set; }
        public EventStatus Status { get; private set; }

        // Relación con categorías/sectores de precios (ej: VIP, General)
        private readonly List<EventSection> _sections = new();
        public IReadOnlyCollection<EventSection> Sections => _sections.AsReadOnly();

        protected Event() { }

        public Event(string title, string description, DateTime date, string venueName, string category)
        {
            if(date < DateTime.UtcNow) throw new ArgumentException("La fecha del evento no puede ser en el pasado.");
            if(string.IsNullOrWhiteSpace(title)) throw new ArgumentException("El título es requerido.");
            if(string.IsNullOrWhiteSpace(category)) throw new ArgumentException("La categoría es requerida.");

            Id = Guid.NewGuid();
            Title = title;
            Description = description;
            Date = date;
            VenueName = venueName;
            Category = category;
            Status = EventStatus.Draft;
        }

        public void UpdateDetails(string title, string description, DateTime date, string venueName, string category)
        {
            if (Status == EventStatus.Cancelled) throw new InvalidOperationException("No se puede modificar un evento cancelado.");
            if (date < DateTime.UtcNow) throw new ArgumentException("La fecha del evento no puede ser en el pasado.");
            if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("El título es requerido.");
            if (string.IsNullOrWhiteSpace(category)) throw new ArgumentException("La categoría es requerida.");

            Title = title;
            Description = description;
            Date = date;
            VenueName = venueName;
            Category = category;
        }

        public void Cancel()
        {
            if (Status == EventStatus.Finished) throw new InvalidOperationException("No se puede cancelar un evento finalizado.");
            Status = EventStatus.Cancelled;
        }

        public void SetImageUrl(string url) => ImageUrl = url;

        public void Publish()
        {
            if (!_sections.Any()) throw new InvalidOperationException("No se puede publicar un evento sin localidades.");
            Status = EventStatus.Published;
        }

        public void AddSection(string name, decimal price, int capacity, bool isNumbered)
        {
            if (Status == EventStatus.Published) throw new InvalidOperationException("No se pueden agregar secciones a un evento publicado.");
            
            var section = new EventSection(Id, name, price, capacity, isNumbered);
            _sections.Add(section);
        }
    }
}
