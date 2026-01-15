using MediatR;
using System.Collections.Generic;
using UsersMS.Domain.Entities;

namespace UsersMS.Application.Queries
{
    public class GetAuditLogsQuery : IRequest<IEnumerable<AuditLog>>
    {
        public int Count { get; set; } = 50;
    }
}
