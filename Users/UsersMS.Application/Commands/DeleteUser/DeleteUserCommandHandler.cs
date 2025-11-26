using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UsersMS.Application.Interfaces;
using UsersMS.Domain.Entities;
using UsersMS.Domain.Interfaces;

namespace UsersMS.Application.Commands.DeleteUser
{
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

        public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (user == null) throw new KeyNotFoundException("Usuario no encontrado.");

            await _keycloakService.DeactivateUserAsync(user.KeycloakId, cancellationToken);

            user.Deactivate();

            var history = new UserHistory(
                user.Id,
                "UserDeactivated",
                "El usuario ha sido desactivado."
            );
            
            await _repository.AddHistoryAsync(history, cancellationToken);  
            
            await _auditService.LogAsync(new AuditLog
            {
                UserId = user.Id.ToString(),
                Action = "DeleteUser",
                Payload = "El usuario ha sido desactivado.",
                IsSuccess = true
            });

            await _repository.SaveChangesAsync(cancellationToken);
        }
    }
}