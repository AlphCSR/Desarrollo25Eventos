using BookingMS.Shared.Enums;
using BookingMS.Domain.Exceptions;
using BookingMS.Domain.ValueObjects;
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
        public Money TotalAmount { get; private set; }
        public Email Email { get; private set; }
        public bool PaymentReminderSent { get; private set; } = false;
        public string? CouponCode { get; private set; }
        public Money DiscountAmount { get; private set; }
        
        private readonly List<Guid> _seatIds = new();
        public IReadOnlyCollection<Guid> SeatIds => _seatIds.AsReadOnly();

        private readonly List<Guid> _serviceIds = new();
        public IReadOnlyCollection<Guid> ServiceIds => _serviceIds.AsReadOnly();

        protected Booking() 
        { 
            TotalAmount = null!;
            Email = null!;
            DiscountAmount = null!;
        }

        public Booking(Guid userId, Guid eventId, List<Guid> seatIds, List<Guid> serviceIds, decimal totalAmount, string email, string? couponCode = null, decimal discountAmount = 0)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            EventId = eventId;
            _seatIds.AddRange(seatIds);
            if (serviceIds != null) _serviceIds.AddRange(serviceIds);
            TotalAmount = (Money)totalAmount;
            Email = (Email)email;
            CouponCode = couponCode;
            DiscountAmount = (Money)discountAmount;
            CreatedAt = DateTime.UtcNow;
            Status = BookingStatus.PendingPayment;
        }

        public void ConfirmPayment()
        {
            if (Status != BookingStatus.PendingPayment) return;
            Status = BookingStatus.Confirmed;
        }

        public void MarkReminderSent()
        {
            PaymentReminderSent = true;
        }

        public DateTime? CancelledAt { get; private set; }

        public void Cancel(string reason)
        {
            if (Status == BookingStatus.Cancelled) 
                throw new InvalidBookingStateException("La reserva ya está cancelada.");
            
            Status = BookingStatus.Cancelled;
            CancelledAt = DateTime.UtcNow;
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