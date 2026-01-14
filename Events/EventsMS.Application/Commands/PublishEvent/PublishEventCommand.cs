using MediatR;
using System;

namespace EventsMS.Application.Commands.PublishEvent
{
    public record PublishEventCommand(Guid Id) : IRequest<bool>;
}
