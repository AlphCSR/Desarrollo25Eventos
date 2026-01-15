using System.Collections.Generic;
using System.Threading.Tasks;
using ReportsMS.Domain.Entities;

namespace ReportsMS.Application.Interfaces
{
    public interface IAuditService
    {
        Task LogAsync(AuditLog log);
        Task<IEnumerable<AuditLog>> GetLatestLogsAsync(int count);
    }
}
