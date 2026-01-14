namespace BookingMS.Shared.Events
{
    // SeatingMS lo escucha para volver a poner el asiento como "Disponible".
    public record BookingCancelledEvent
    {
        public Guid BookingId { get; init; }      // Identificador de la reserva cancelada
        public Guid EventSeatId { get; init; }    // Identificador del asiento asociado
        public string Reason { get; init; } = string.Empty; // Razón de la cancelación (ej. "Expired", "UserCancelled")
        public List<Guid> SeatIds { get; init; } = new List<Guid>(); // IDs de los asientos involucrados
    }
}