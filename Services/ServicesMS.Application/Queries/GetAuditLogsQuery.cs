using MediatR;
using System.Collections.Generic;
using ServicesMS.Domain.Entities;

namespace ServicesMS.Application.Queries
{
    public class GetAuditLogsQuery : IRequest<IEnumerable<AuditLog>>
    {
        public int Count { get; set; } = 50;
    }
}
