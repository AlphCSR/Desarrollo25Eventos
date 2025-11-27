using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using UsersMS.Application.Commands;
using UsersMS.Domain.Interfaces;
using UsersMS.Domain.Exceptions;
using UsersMS.Domain.Entities; // Added for UserHistory

namespace UsersMS.Application.Commands.UpdateUserPreferences
{
    public class UpdateUserPreferencesCommandHandler : IRequestHandler<UpdateUserPreferencesCommand>
    {
        private readonly IUserRepository _userRepository;

        public UpdateUserPreferencesCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task Handle(UpdateUserPreferencesCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            
            if (user == null) 
                throw new UserNotFoundException($"Usuario con ID {request.UserId} no encontrado.");

            // Actualizar Dominio
            var oldPrefs = string.Join(", ", user.Preferences);
            user.UpdatePreferences(request.Data.Preferences);
            var newPrefs = string.Join(", ", user.Preferences);

            if (oldPrefs != newPrefs)
            {
                var history = new UserHistory(
                    user.Id,
                    "PreferencesUpdated",
                    $"Preferencias cambiadas de [{oldPrefs}] a [{newPrefs}]"
                );
                await _userRepository.AddHistoryAsync(history, cancellationToken);
            }

            // Guardar (El repositorio maneja el ChangeTracking)
            // await _userRepository.UpdateAsync(user, cancellationToken); // REMOVED to avoid concurrency exception
            await _userRepository.SaveChangesAsync(cancellationToken);
        }
    }
}