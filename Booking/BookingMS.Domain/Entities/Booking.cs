using BookingMS.Shared.Enums;
using BookingMS.Domain.Exceptions;
using System;
using System.Collections.Generic;

namespace BookingMS.Domain.Entities
{
    public class Booking
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public Guid EventId { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public BookingStatus Status { get; private set; }
        public decimal TotalAmount { get; private set; }
        
        // Lista de IDs de los asientos reservados
        private readonly List<Guid> _seatIds = new();
        public IReadOnlyCollection<Guid> SeatIds => _seatIds.AsReadOnly();

        protected Booking() { }

        public Booking(Guid userId, Guid eventId, List<Guid> seatIds, decimal totalAmount)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            EventId = eventId;
            _seatIds.AddRange(seatIds);
            TotalAmount = totalAmount;
            CreatedAt = DateTime.UtcNow;
            Status = BookingStatus.PendingPayment; // Estado inicial
        }

        public void ConfirmPayment()
        {
            if (Status != BookingStatus.PendingPayment) return;
            Status = BookingStatus.Confirmed;
        }

        public void Cancel(string reason)
        {
            if (Status == BookingStatus.Confirmed) 
                throw new InvalidBookingStateException("No se puede cancelar una reserva ya pagada automáticamente.");
            
            Status = BookingStatus.Cancelled;
        }

        public void RemoveSeat(Guid seatId)
        {
            if (Status != BookingStatus.PendingPayment)
                throw new InvalidBookingStateException("Solo se pueden remover asientos de reservas pendientes.");

            if (_seatIds.Contains(seatId))
            {
                _seatIds.Remove(seatId);
            }
        }
    }
}