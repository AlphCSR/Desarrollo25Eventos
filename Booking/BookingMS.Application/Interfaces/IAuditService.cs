using System.Collections.Generic;
using System.Threading.Tasks;
using BookingMS.Domain.Entities;

namespace BookingMS.Application.Interfaces
{
    public interface IAuditService
    {
        Task LogAsync(AuditLog log);
        Task<IEnumerable<AuditLog>> GetLatestLogsAsync(int count);
    }
}
