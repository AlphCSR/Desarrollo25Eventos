using System;
using System.Collections.Generic;
using UsersMS.Domain.Common;
using UsersMS.Domain.ValueObjects;
using UsersMS.Shared.Enums;
using UsersMS.Domain.Exceptions;

namespace UsersMS.Domain.Entities
{
    /// <summary>
    /// Representa un usuario en el sistema.
    /// </summary>
    public class User : BaseEntity
    {
        public string FullName { get; private set; }
        public string Email { get; private set; }
        public string KeycloakId { get; private set; }
        public UserRole Role { get; private set; }
        public UserState State { get; private set; }
        public List<string> Preferences { get; private set; } = new();
        private readonly List<UserHistory> _history = new();
        
        /// <summary>
        /// Historial de cambios del usuario.
        /// </summary>
        public IReadOnlyCollection<UserHistory> History => _history.AsReadOnly();

        protected User() { }

        /// <summary>
        /// Constructor para crear un nuevo usuario.
        /// </summary>
        /// <param name="fullName">Nombre completo del usuario.</param>
        /// <param name="email">Correo electrónico del usuario.</param>
        /// <param name="keycloakId">ID del usuario en Keycloak.</param>
        /// <param name="role">Rol del usuario.</param>
        /// <exception cref="InvalidUserDataException">Lanzada si los datos son inválidos.</exception>
        public User(string fullName, string email, string keycloakId, UserRole role)
        {
            if (string.IsNullOrWhiteSpace(fullName)) throw new InvalidUserDataException("El nombre es requerido.");
            if (string.IsNullOrWhiteSpace(keycloakId)) throw new InvalidUserDataException("El ID de Keycloak es requerido.");

            FullName = fullName;
            Email = ValueObjects.Email.Create(email).Value;
            KeycloakId = keycloakId;
            Role = role;
            State = UserState.Active;

            AddHistory("UserCreated", $"Usuario creado con rol {role}");
        }

        /// <summary>
        /// Actualiza las preferencias del usuario.
        /// </summary>
        /// <param name="newPreferences">Nueva lista de preferencias.</param>
        /// <exception cref="InvalidUserDataException">Lanzada si la lista de preferencias es nula.</exception>
        public void UpdatePreferences(List<string> newPreferences)
        {
            if (newPreferences == null) 
                throw new InvalidUserDataException("La lista de preferencias no puede ser nula.");

            // Detectar cambios para el historial
            var oldPrefs = string.Join(", ", Preferences ?? new List<string>());
            var newPrefs = string.Join(", ", newPreferences);

            if (oldPrefs != newPrefs)
            {
                Preferences = newPreferences; // Reemplazamos la lista
                UpdatedAt = DateTime.UtcNow;
            }
        }
    
        /// <summary>
        /// Actualiza el perfil del usuario.
        /// </summary>
        /// <param name="newName">Nuevo nombre completo.</param>
        /// <exception cref="InvalidUserDataException">Lanzada si el nombre está vacío.</exception>
        public void UpdateProfile(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName)) throw new InvalidUserDataException("El nombre no puede estar vacío.");

            var oldName = FullName ?? "";
            FullName = newName ?? "";
            UpdatedAt = DateTime.UtcNow ;
        }

        private void AddHistory(string action, string details)
        {
            _history.Add(new UserHistory(this.Id, action, details));
        }

        /// <summary>
        /// Desactiva al usuario.
        /// </summary>
        public void Deactivate()
        {
            if (State == UserState.Inactive) return;
            
            State = UserState.Inactive;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}