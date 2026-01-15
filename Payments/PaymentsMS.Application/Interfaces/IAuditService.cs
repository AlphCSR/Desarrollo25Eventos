using System.Collections.Generic;
using System.Threading.Tasks;
using PaymentsMS.Domain.Entities;

namespace PaymentsMS.Application.Interfaces
{
    public interface IAuditService
    {
        Task LogAsync(AuditLog log);
        Task<IEnumerable<AuditLog>> GetLatestLogsAsync(int count);
    }
}
