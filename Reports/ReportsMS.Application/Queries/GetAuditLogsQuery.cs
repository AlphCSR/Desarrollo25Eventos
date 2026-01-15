using MediatR;
using System.Collections.Generic;
using ReportsMS.Domain.Entities;

namespace ReportsMS.Application.Queries
{
    public class GetAuditLogsQuery : IRequest<IEnumerable<AuditLog>>
    {
        public int Count { get; set; } = 50;
    }
}
