using MediatR;
using EventsMS.Application.DTOs;
using System;
using System.Collections.Generic;

namespace EventsMS.Application.Queries.GetEventSections
{
    public record GetEventSectionsQuery(Guid EventId) : IRequest<IEnumerable<EventSectionDto>>;
}
