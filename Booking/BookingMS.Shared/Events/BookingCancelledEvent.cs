namespace BookingMS.Shared.Events
{
    public record BookingCancelledEvent
    {
        public Guid BookingId { get; init; }      
        public Guid UserId { get; init; }         
        public Guid EventSeatId { get; init; }    
        public string Reason { get; init; } = string.Empty; 
        public string Email { get; init; } = string.Empty;
        public List<Guid> SeatIds { get; init; } = new List<Guid>(); 
        public string Language { get; init; } = "es";
        public string EventName { get; init; } = "Evento";
    }
}