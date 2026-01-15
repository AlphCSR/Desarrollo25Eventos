using System;

namespace SeatingMS.Domain.Entities 
{

    public class AuditLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserId { get; set; }
        public string Action { get; set; }
        public string Payload { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
