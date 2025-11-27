using MediatR;
using EventsMS.Application.DTOs;
using System.Collections.Generic;

namespace EventsMS.Application.Queries.GetEvents
{
    public class GetEventsQuery : IRequest<List<EventDto>>
    {
    }
}
