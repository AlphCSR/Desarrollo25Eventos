using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using UsersMS.Application.Interfaces;
using UsersMS.Domain.Entities;
using UsersMS.Domain.Interfaces;
using UsersMS.Domain.Exceptions;
using System.Collections.Generic;

namespace UsersMS.Application.Commands.UpdateUser
{
    /// <summary>
    /// Manejador para el comando de actualización de usuario.
    /// </summary>
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand>
    {
        private readonly IUserRepository _repository;
        private readonly IKeycloakService _keycloakService;
        private readonly IAuditService _auditService;

        public UpdateUserCommandHandler(IUserRepository repository, IKeycloakService keycloakService, IAuditService auditService)
        {
            _repository = repository;
            _keycloakService = keycloakService;
            _auditService = auditService;
        }

        /// <summary>
        /// Maneja la lógica de actualización de un usuario.
        /// </summary>
        /// <param name="request">Comando de actualización de usuario.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <exception cref="UserNotFoundException">Lanzada si el usuario no existe.</exception>
        public async Task Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            // 1. Obtener usuario
            var user = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (user == null) throw new UserNotFoundException($"Usuario con ID {request.Id} no encontrado.");

            // 2. Actualizar Keycloak
            var names = request.Data.FullName.Split(' ', 2);
            var firstName = names[0];
            var lastName = names.Length > 1 ? names[1] : "";

            await _keycloakService.UpdateUserAsync(user.KeycloakId, firstName, lastName, cancellationToken);

            // 3. Guardar estado anterior para el historial 
            var oldName = user.FullName;

            // 4. Actualizar entidad User 
            user.UpdateProfile(request.Data.FullName);
            
            // 5. Crear Historial 
            var history = new UserHistory(
                user.Id, 
                "ProfileUpdated", 
                $"Nombre cambiado de '{oldName}' a '{request.Data.FullName}'"
            );

            // 6. Persistencia Explícita
            // Marcamos el usuario como modificado
            
            // Agregamos el historial 
            await _repository.AddHistoryAsync(history, cancellationToken);

            // Log Audit
            await _auditService.LogAsync(new AuditLog
            {
                UserId = user.Id.ToString(),
                Action = "UpdateUser",
                Payload = $"Nombre cambiado de '{oldName}' a '{request.Data.FullName}'",
                IsSuccess = true
            });

            // 7. Guardar todo
            await _repository.SaveChangesAsync(cancellationToken);
        }
    }
}