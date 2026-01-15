using MediatR;
using System;

namespace BookingMS.Application.Commands.CancelBooking;

public record CancelBookingCommand(Guid BookingId, string Reason = "User Cancelled", string Language = "es") : IRequest<bool>;
