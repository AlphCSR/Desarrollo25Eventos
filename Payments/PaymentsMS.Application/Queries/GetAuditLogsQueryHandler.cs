using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using PaymentsMS.Domain.Entities;
using PaymentsMS.Application.Interfaces;

namespace PaymentsMS.Application.Queries
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
