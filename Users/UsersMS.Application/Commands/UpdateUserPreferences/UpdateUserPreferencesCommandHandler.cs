using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using UsersMS.Application.Commands;
using UsersMS.Domain.Interfaces;
using UsersMS.Domain.Exceptions;
using UsersMS.Domain.Entities; 

namespace UsersMS.Application.Commands.UpdateUserPreferences
{
    /// <summary>
    /// Manejador para el comando de actualización de preferencias de usuario.
    /// </summary>
    public class UpdateUserPreferencesCommandHandler : IRequestHandler<UpdateUserPreferencesCommand>
    {
        private readonly IUserRepository _userRepository;

        public UpdateUserPreferencesCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        /// <summary>
        /// Maneja la actualización de preferencias de usuario.
        /// </summary>
        /// <param name="request">Comando de actualización de preferencias.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        public async Task Handle(UpdateUserPreferencesCommand request, CancellationToken cancellationToken)
        {
            //1. Buscar el usuario
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            
            //2. Validar que el usuario exista
            if (user == null) 
                throw new UserNotFoundException($"Usuario con ID {request.UserId} no encontrado.");

            //3. Actualizar preferencias
            var oldPrefs = string.Join(", ", user.Preferences);
            user.UpdatePreferences(request.Data.Preferences);
            var newPrefs = string.Join(", ", user.Preferences);

            //4. Generar historial
            if (oldPrefs != newPrefs)
            {
                var history = new UserHistory(
                    user.Id,
                    "PreferencesUpdated",
                    $"Preferencias cambiadas de [{oldPrefs}] a [{newPrefs}]"
                );
                await _userRepository.AddHistoryAsync(history, cancellationToken);
            }

            //5. Guardar cambios
            await _userRepository.SaveChangesAsync(cancellationToken);
        }
    }
}