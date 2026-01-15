using MediatR;
using System.Collections.Generic;
using PaymentsMS.Domain.Entities;

namespace PaymentsMS.Application.Queries
{
    public class GetAuditLogsQuery : IRequest<IEnumerable<AuditLog>>
    {
        public int Count { get; set; } = 50;
    }
}
