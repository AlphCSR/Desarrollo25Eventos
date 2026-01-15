using System.Collections.Generic;
using System.Threading.Tasks;
using UsersMS.Domain.Entities;

namespace UsersMS.Application.Interfaces
{
    public interface IAuditService
    {
        Task LogAsync(AuditLog log);
        Task<IEnumerable<AuditLog>> GetLatestLogsAsync(int count);
    }
}
