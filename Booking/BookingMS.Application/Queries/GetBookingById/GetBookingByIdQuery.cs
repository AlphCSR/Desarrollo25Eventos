using BookingMS.Shared.Dtos.Response;
using MediatR;
using System;

namespace BookingMS.Application.Queries.GetBookingById;

public record GetBookingByIdQuery(Guid Id) : IRequest<BookingDto?>;
