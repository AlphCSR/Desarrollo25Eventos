using MediatR;
using System.Collections.Generic;
using SeatingMS.Domain.Entities;

namespace SeatingMS.Application.Queries
{
    public class GetAuditLogsQuery : IRequest<IEnumerable<AuditLog>>
    {
        public int Count { get; set; } = 50;
    }
}
