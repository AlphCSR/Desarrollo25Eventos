using MediatR;
using System.Collections.Generic;
using EventsMS.Domain.Entities;

namespace EventsMS.Application.Queries
{
    public class GetAuditLogsQuery : IRequest<IEnumerable<AuditLog>>
    {
        public int Count { get; set; } = 50;
    }
}
