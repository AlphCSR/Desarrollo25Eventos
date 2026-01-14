using BookingMS.Shared.Dtos.Response;
using System;
using System.Collections.Generic;

namespace BookingMS.Shared.Events
{
    public class BookingConfirmedEvent
    {
        public Guid BookingId { get; set; }
        public Guid UserId { get; set; }
        public Guid EventId { get; set; }
        public decimal TotalAmount { get; set; }
        public List<Guid> SeatIds { get; set; } = new();
        public List<Guid> ServiceIds { get; set; } = new();
        public List<InvoiceItemDto> Items { get; set; } = new();
        public DateTime ConfirmedAt { get; set; }
        public string Email { get; set; } = string.Empty;
        public string EventName { get; set; } = "Evento";
        public string? CouponCode { get; set; }
        public decimal DiscountAmount { get; set; }
        public string Language { get; set; } = "es";
    }
}
