using Microsoft.EntityFrameworkCore;
using UsersMS.Domain.Entities;
using UsersMS.Domain.Interfaces;
using UsersMS.Infrastructure.Persistence;

namespace UsersMS.Infrastructure.Repositories
{
    /// <summary>
    /// Implementación de IUserRepository que utiliza Entity Framework Core para interactuar con la base de datos.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly UsersDbContext _context;

        public UserRepository(UsersDbContext context)
        {
            _context = context;
        }

        /// <summary> 
        /// Obtiene un usuario por su ID 
        /// <param name="id">El ID del usuario</param>
        /// <returns>El usuario con el ID especificado</returns>
        /// </summary>
        public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .Include(u => u.History)
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }

        /// <summary>
        /// Obtiene un usuario por su correo electrónico
        /// </summary>
        /// <param name="email">El correo electrónico del usuario</param>
        /// <returns>El usuario con el correo electrónico especificado</returns>
        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        }

        /// <summary>
        /// Agrega un nuevo usuario a la base de datos.
        /// </summary>
        public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            await _context.Users.AddAsync(user, cancellationToken);
        }

        /// <summary>
        /// Actualiza un usuario en la base de datos.
        /// </summary>
        public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            _context.Users.Update(user);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Agrega un nuevo historial de usuario a la base de datos.
        /// </summary>
        public async Task AddHistoryAsync(UserHistory history, CancellationToken cancellationToken = default)
        {
            await _context.Set<UserHistory>().AddAsync(history, cancellationToken);
        }

        /// <summary>
        /// Guarda los cambios en la base de datos.
        /// </summary>
        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene todos los usuarios de la base de datos.
        /// </summary>
        public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Users.ToListAsync(cancellationToken);
        }
    }
}