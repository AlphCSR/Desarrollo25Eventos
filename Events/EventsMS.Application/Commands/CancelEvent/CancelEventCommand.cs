using MediatR;
using System;

namespace EventsMS.Application.Commands.CancelEvent;

public record CancelEventCommand(Guid EventId, Guid UserId, bool IsAdmin, string Reason = "Organizer Cancelled") : IRequest<bool>;
