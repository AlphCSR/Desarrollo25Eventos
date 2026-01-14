using System.Collections.Generic;
using System.Threading.Tasks;
using SeatingMS.Domain.Entities;

namespace SeatingMS.Application.Interfaces
{
    public interface IAuditService
    {
        Task LogAsync(AuditLog log);
        Task<IEnumerable<AuditLog>> GetLatestLogsAsync(int count);
    }
}
