using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using UsersMS.Application.Interfaces;
using UsersMS.Domain.Entities;
using UsersMS.Domain.Interfaces;
using UsersMS.Domain.Exceptions;

using System.Collections.Generic;
using MassTransit;
using UsersMS.Shared.Events;

namespace UsersMS.Application.Commands.UpdateUser
{
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand>
    {
        private readonly IUserRepository _repository;
        private readonly IKeycloakService _keycloakService;
        private readonly IAuditService _auditService;
        private readonly IPublishEndpoint _publishEndpoint;

        public UpdateUserCommandHandler(IUserRepository repository, IKeycloakService keycloakService, IAuditService auditService, IPublishEndpoint publishEndpoint)
        {
            _repository = repository;
            _keycloakService = keycloakService;
            _auditService = auditService;
            _publishEndpoint = publishEndpoint;
        }

        public async Task Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (user == null) throw new UserNotFoundException($"Usuario con ID {request.Id} no encontrado.");

            var oldName = user.FullName;
            var oldPhone = user.PhoneNumber;
            var oldDocument = user.DocumentId;
            var oldDob = user.DateOfBirth;
            var oldAddress = user.Address;
            var oldProfilePic = user.ProfilePictureUrl;
            var oldLanguage = user.Language;
            var oldPreferences = string.Join(", ", user.Preferences ?? new List<string>());

            if (oldName != request.Data.FullName)
            {
                var names = request.Data.FullName.Split(' ', 2);
                var firstName = names[0];
                var lastName = names.Length > 1 ? names[1] : "";
                await _keycloakService.UpdateUserAsync(user.KeycloakId, firstName, lastName, cancellationToken);
            }

            user.UpdateProfile(
                request.Data.FullName,
                request.Data.PhoneNumber,
                request.Data.DocumentId,
                request.Data.DateOfBirth ?? user.DateOfBirth,
                request.Data.Address,
                request.Data.ProfilePictureUrl ?? user.ProfilePictureUrl,
                request.Data.Language ?? user.Language
            );

            if (request.Data.Preferences != null)
            {
                user.UpdatePreferences(request.Data.Preferences);
            }

            var changes = new List<string>();

            if (oldName != user.FullName) changes.Add($"Nombre cambiado de '{oldName}' a '{user.FullName}'");
            if (oldPhone != user.PhoneNumber) changes.Add($"Teléfono cambiado de '{oldPhone ?? "null"}' a '{user.PhoneNumber ?? "null"}'");
            if (oldDocument != user.DocumentId) changes.Add($"Documento cambiado de '{oldDocument ?? "null"}' a '{user.DocumentId ?? "null"}'");
            if (oldDob != user.DateOfBirth) changes.Add($"Fecha de nacimiento cambiada de '{oldDob?.ToString("d") ?? "null"}' a '{user.DateOfBirth?.ToString("d") ?? "null"}'");
            if (oldAddress != user.Address) changes.Add($"Dirección cambiada de '{oldAddress ?? "null"}' a '{user.Address ?? "null"}'");
            if (oldProfilePic != user.ProfilePictureUrl) changes.Add("Foto de perfil actualizada");
            if (oldLanguage != user.Language) changes.Add($"Idioma cambiado de '{oldLanguage}' a '{user.Language}'");
            
            var newPreferences = string.Join(", ", user.Preferences ?? new List<string>());
            if (oldPreferences != newPreferences) changes.Add($"Preferencias cambiadas de [{oldPreferences}] a [{newPreferences}]");

            if (changes.Count > 0)
            {
                var details = "Perfil actualizado: " + string.Join("; ", changes);

                await _publishEndpoint.Publish(new UserHistoryCreatedEvent(
                    user.Id, 
                    "ProfileUpdated", 
                    details,
                    DateTime.UtcNow
                ), cancellationToken);

                await _publishEndpoint.Publish(new UserProfileUpdatedEvent(
                    user.Id,
                    user.FullName,
                    user.Preferences,
                    user.PhoneNumber,
                    user.DocumentId,
                    user.DateOfBirth,
                    user.Address,
                    user.Language
                ), cancellationToken);

                await _auditService.LogAsync(new AuditLog
                {
                    UserId = user.Id.ToString(),
                    Action = "UpdateUser",
                    Payload = details,
                    IsSuccess = true
                });
            }

            await _repository.SaveChangesAsync(cancellationToken);
        }
    }
}