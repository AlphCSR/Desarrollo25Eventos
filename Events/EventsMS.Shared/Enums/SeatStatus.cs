namespace EventsMS.Shared.Enums
{
    public enum SeatStatus
    {
        Available, // Libre para comprar
        Locked,    // Bloqueado temporalmente (alguien lo está comprando)
        Booked,    // Vendido/Reservado
        Unavailable // No disponible (ej. dañado o reservado por organización)
    }
}