using System;
using System.Collections.Generic;
using System.Linq;
using EventsMS.Shared.Enums;
using EventsMS.Domain.Exceptions;
using EventsMS.Domain.ValueObjects;

namespace EventsMS.Domain.Entities
{
    public class Event 
    {
        public Guid Id { get; private set; }
        public Guid IdUser { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        public DateRange DateRange { get; private set; }
        public DateTime Date => DateRange.StartDate;
        public DateTime EndDate => DateRange.EndDate;
        public string VenueName { get; private set; }
        public string? ImageUrl { get; private set; }
        public List<string> Categories { get; private set; } = new();
        public EventStatus Status { get; private set; }
        public EventType Type { get; private set; } 
        public string? StreamingUrl { get; private set; } 

        private readonly List<EventSection> _sections = new();
        public IReadOnlyCollection<EventSection> Sections => _sections.AsReadOnly();

        protected Event() 
        { 
            Title = null!;
            Description = null!;
            DateRange = null!;
            VenueName = null!;
        }

        public Event(Guid idUser, string title, string description, DateTime date, DateTime endDate, string venueName, List<string> categories, EventType type = EventType.Physical, string? streamingUrl = null)
        {
            if(string.IsNullOrWhiteSpace(title)) throw new InvalidEventDataException("El título es requerido.");
            if(categories == null || !categories.Any()) throw new InvalidEventDataException("Al menos una categoría es requerida.");

            Id = Guid.NewGuid();
            IdUser = idUser;
            Title = title;
            Description = description;
            DateRange = DateRange.Create(date, endDate);
            VenueName = venueName;
            Categories = categories;
            Status = EventStatus.Draft;
            Type = type;
            StreamingUrl = streamingUrl;
        }

        public void UpdateDetails(string title, string description, DateTime date, DateTime endDate, string venueName, List<string> categories, EventType type, string? streamingUrl)
        {
            if (Status == EventStatus.Cancelled) throw new InvalidEventDataException("No se puede modificar un evento cancelado.");
            if (string.IsNullOrWhiteSpace(title)) throw new InvalidEventDataException("El título es requerido.");
            if (categories == null || !categories.Any()) throw new InvalidEventDataException("Al menos una categoría es requerida.");

            Title = title;
            Description = description;
            DateRange = DateRange.Create(date, endDate);
            VenueName = venueName;
            Categories = categories;
            Type = type;
            StreamingUrl = streamingUrl;
        }

        public void Cancel()
        {
            if (Status == EventStatus.Finished) throw new InvalidOperationException("No se puede cancelar un evento finalizado.");
            Status = EventStatus.Cancelled;
        }

        public void Start()
        {
            if (Status != EventStatus.Published) return;
            Status = EventStatus.Live;
        }

        public void Finish()
        {
            if (Status != EventStatus.Published && Status != EventStatus.Live) return; 
            Status = EventStatus.Finished;
        }

        public void ResetToPublished()
        {
            if (Status == EventStatus.Live)
            {
                Status = EventStatus.Published;
            }
        }

        public void SetImageUrl(string url) => ImageUrl = url;

        public void Publish()
        {
            if (!_sections.Any()) throw new InvalidOperationException("No se puede publicar un evento sin localidades.");
            Status = EventStatus.Published;
        }

        public void UpdateStatus(EventStatus newStatus)
        {
            Status = newStatus;
        }

        public void AddSection(string name, decimal price, int capacity, bool isNumbered)
        {
            if (Status == EventStatus.Published) throw new InvalidOperationException("No se pueden agregar secciones a un evento publicado.");
            
            var section = new EventSection(Id, name, price, capacity, isNumbered);
            _sections.Add(section);
        }
    }
}
