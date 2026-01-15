using MediatR;
using EventsMS.Shared.Enums;
using System;

namespace EventsMS.Application.Commands.UpdateEventStatus
{
    public record UpdateEventStatusCommand(Guid Id, EventStatus NewStatus) : IRequest<bool>;
}
