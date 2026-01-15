using System.Collections.Generic;
using System.Threading.Tasks;
using ServicesMS.Domain.Entities;

namespace ServicesMS.Application.Interfaces
{
    public interface IAuditService
    {
        Task LogAsync(AuditLog log);
        Task<IEnumerable<AuditLog>> GetLatestLogsAsync(int count);
    }
}
