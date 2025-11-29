using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UsersMS.Application.Interfaces;
using UsersMS.Domain.Entities;
using UsersMS.Domain.Interfaces;
using UsersMS.Domain.Exceptions;

namespace UsersMS.Application.Commands.DeleteUser
{
    /// <summary>
    /// Manejador para el comando de eliminación (desactivación) de usuario.
    /// </summary>
    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
    {
        private readonly IUserRepository _repository;
        private readonly IKeycloakService _keycloakService;
        private readonly IAuditService _auditService;

        public DeleteUserCommandHandler(IUserRepository repository, IKeycloakService keycloakService, IAuditService auditService)
        {
            _repository = repository;
            _keycloakService = keycloakService;
            _auditService = auditService;
        }

        /// <summary>
        /// Maneja la lógica de desactivación de un usuario.
        /// </summary>
        /// <param name="request">Comando de eliminación de usuario.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <exception cref="UserNotFoundException">Lanzada si el usuario no existe.</exception>
        public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            //1. Buscar el usuario
            var user = await _repository.GetByIdAsync(request.Id, cancellationToken);
            
            //2. Validar que el usuario exista
            if (user == null) throw new UserNotFoundException("Usuario no encontrado.");

            //3. Desactivar el usuario en Keycloak
            await _keycloakService.DeactivateUserAsync(user.KeycloakId, cancellationToken);

            user.Deactivate();

            var history = new UserHistory(
                user.Id,
                "UserDeactivated",
                "El usuario ha sido desactivado."
            );
            
            //4. Agregar el historial
            await _repository.AddHistoryAsync(history, cancellationToken);  
            
            //5. Registrar en Audit
            await _auditService.LogAsync(new AuditLog
            {
                UserId = user.Id.ToString(),
                Action = "DeleteUser",
                Payload = "El usuario ha sido desactivado.",
                IsSuccess = true
            });

            //6. Guardar cambios
            await _repository.SaveChangesAsync(cancellationToken);
        }
    }
}