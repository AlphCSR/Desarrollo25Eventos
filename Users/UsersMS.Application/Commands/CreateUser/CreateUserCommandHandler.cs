using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using UsersMS.Application.Commands;
using UsersMS.Domain.Entities;
using UsersMS.Domain.Interfaces;
using UsersMS.Application.Interfaces;
using UsersMS.Shared.Enums;
using UsersMS.Domain.Exceptions;

namespace UsersMS.Application.Commands.CreateUser
{
    /// <summary>
    /// Manejador para el comando de creación de usuario.
    /// </summary>
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
    {
        private readonly IUserRepository _userRepository;
        private readonly IKeycloakService _keycloakService;

        public CreateUserCommandHandler(IUserRepository userRepository, IKeycloakService keycloakService)
        {
            _userRepository = userRepository;
            _keycloakService = keycloakService;
        }

        /// <summary>
        /// Maneja la lógica de creación de un nuevo usuario.
        /// </summary>
        /// <param name="request">Comando de creación de usuario.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>ID del usuario creado.</returns>
        /// <exception cref="UserAlreadyExistsException">Lanzada si el usuario ya existe.</exception>
        public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            // 1. Verificar si el usuario ya existe en la BD local
            var existingUser = await _userRepository.GetByEmailAsync(request.UserData.Email, cancellationToken);
            if (existingUser != null)
            {
                throw new UserAlreadyExistsException($"El usuario con email {request.UserData.Email} ya existe.");
            }

            //2. Crear el usuario en Keycloak
            var names = request.UserData.FullName.Split(' ', 2);
            var firstName = names[0];
            var lastName = names.Length > 1 ? names[1] : "";

            string keycloakId = await _keycloakService.RegisterUserAsync(
                request.UserData.Email, 
                request.UserData.Password, 
                firstName, 
                lastName, 
                cancellationToken);

            await _keycloakService.AssignRoleAsync(request.UserData.Email, request.UserData.Role.ToString(), cancellationToken);

            var newUser = new User(
                request.UserData.FullName,
                request.UserData.Email,
                keycloakId,
                request.UserData.Role
            );

            //3. Guardar el usuario en la base de datos
            await _userRepository.AddAsync(newUser, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            return newUser.Id;
        }
    }
}