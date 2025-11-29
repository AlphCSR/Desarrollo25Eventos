using System.Threading.Tasks;
using UsersMS.Domain.Entities;

namespace UsersMS.Application.Interfaces
{
    public interface IAuditService
    {
        Task LogAsync(AuditLog log);
    }
}
