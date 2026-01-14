using MediatR;
using ServicesMS.Application.DTOs;
using System;

namespace ServicesMS.Application.Commands
{
    public record BookServiceCommand(BookServiceDto BookingData) : IRequest<bool>;
}
