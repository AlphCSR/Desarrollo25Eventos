using MediatR;
using BookingMS.Shared.Dtos.Response;
using System;
using System.Collections.Generic;

namespace BookingMS.Application.Commands.CreateBooking
{
    // Recibimos los datos necesarios. El frontend debe enviar el precio calculado (o recalculamos aquí consultando EventsMS)
    public record CreateBookingCommand(Guid UserId, Guid EventId, List<Guid> SeatIds, decimal TotalAmount) : IRequest<BookingDto>;
}