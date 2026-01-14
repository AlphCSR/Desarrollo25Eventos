using System.Threading;
using System.Threading;
using System.Threading.Tasks;

namespace UsersMS.Application.Interfaces
{
    public interface IKeycloakService
    {
        Task<string> RegisterUserAsync(string email, string password, string firstName, string lastName, CancellationToken cancellationToken);
        Task AssignRoleAsync(string username, string role, CancellationToken cancellationToken);
        Task UpdateUserAsync(string keycloakId, string firstName, string lastName, CancellationToken cancellationToken);
        Task DeactivateUserAsync(string keycloakId, CancellationToken cancellationToken);
    }
}