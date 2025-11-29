using System;
using EventsMS.Shared.Enums;

namespace EventsMS.Application.DTOs
{
    public class EventDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public string VenueName { get; set; }
        public string? ImageUrl { get; set; }
        public EventStatus Status { get; set; }
        public string Category { get; set; }
    }
}
