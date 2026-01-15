using System;
using EventsMS.Shared.Enums;

namespace EventsMS.Application.DTOs
{
    public class EventDto
    {
        public Guid Id { get; set; }
        public Guid IdUser { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public DateTime EndDate { get; set; }
        public string VenueName { get; set; }
        public string? ImageUrl { get; set; }
        public EventStatus Status { get; set; }
        public EventType Type { get; set; }
        public string? StreamingUrl { get; set; }
        public List<string> Categories { get; set; } = new();
        public decimal MinPrice { get; set; }
    }
}
