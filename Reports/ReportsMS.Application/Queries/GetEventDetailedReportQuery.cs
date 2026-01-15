using MediatR;
using ReportsMS.Application.DTOs;
using System;

namespace ReportsMS.Application.Queries
{
    public class GetEventDetailedReportQuery : IRequest<EventDetailedReportDto?>
    {
        public Guid EventId { get; }

        public GetEventDetailedReportQuery(Guid eventId)
        {
            EventId = eventId;
        }
    }
}
