using System;
using System.Collections.Generic;

namespace EventsMS.Shared.Events
{
    public class EventUpdatedEvent
    {
        public Guid EventId { get; set; }
        public Guid IdUser { get; set; }
        public string Title { get; set; }
        public string Changes { get; set; } 
        public DateTime UpdatedAt { get; set; }
    }
}
