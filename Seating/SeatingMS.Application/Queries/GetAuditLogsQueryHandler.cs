using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using SeatingMS.Domain.Entities;
using SeatingMS.Application.Interfaces;

namespace SeatingMS.Application.Queries
{
    public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, IEnumerable<AuditLog>>
    {
        private readonly IAuditService _auditService;

        public GetAuditLogsQueryHandler(IAuditService auditService)
        {
            _auditService = auditService;
        }

        public async Task<IEnumerable<AuditLog>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
        {
            return await _auditService.GetLatestLogsAsync(request.Count);
        }
    }
}
