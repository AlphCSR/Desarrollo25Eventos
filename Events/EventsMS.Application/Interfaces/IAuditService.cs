using System.Collections.Generic;
using System.Threading.Tasks;
using EventsMS.Domain.Entities;

namespace EventsMS.Application.Interfaces
{
    public interface IAuditService
    {
        Task LogAsync(AuditLog log);
        Task<IEnumerable<AuditLog>> GetLatestLogsAsync(int count);
    }
}
