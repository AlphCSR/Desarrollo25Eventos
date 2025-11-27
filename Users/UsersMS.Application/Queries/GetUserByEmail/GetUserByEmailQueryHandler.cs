using System.Diagnostics.CodeAnalysis;
using MediatR;
using UsersMS.Application.DTOs;
using UsersMS.Domain.Interfaces;
using UsersMS.Domain.Exceptions;

namespace UsersMS.Application.Queries.GetUserByEmail
{
    /// <summary>
    /// Manejador para la consulta de usuario por email.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class GetUserByEmailQueryHandler : IRequestHandler<GetUserByEmailQuery, UserDto?>
    {
        private readonly IUserRepository _userRepository;

        public GetUserByEmailQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        /// <summary>
        /// Maneja la lógica de obtención de un usuario por su email.
        /// </summary>
        /// <param name="request">Consulta de usuario por email.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>DTO del usuario encontrado.</returns>
        /// <exception cref="UserNotFoundException">Lanzada si el usuario no existe.</exception>
        public async Task<UserDto> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user == null) throw new UserNotFoundException("User not found");
            return new UserDto(user.Id, user.FullName, user.Email, user.KeycloakId, user.Role, user.State);
        }
    }
}
