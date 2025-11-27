using System;
using System.Collections.Generic;
using UsersMS.Domain.Common;
using UsersMS.Domain.ValueObjects;
using UsersMS.Shared.Enums;

namespace UsersMS.Domain.Entities
{
    public class User : BaseEntity
    {
        public string FullName { get; private set; }
        public string Email { get; private set; }
        public string KeycloakId { get; private set; }
        public UserRole Role { get; private set; }
        public UserState State { get; private set; }
        public List<string> _preferences = new();
        private readonly List<UserHistory> _history = new();
        public IReadOnlyCollection<string> Preferences => (_preferences ?? new List<string>()).AsReadOnly();
        public IReadOnlyCollection<UserHistory> History => _history.AsReadOnly();

        protected User() { }

        public User(string fullName, string email, string keycloakId, UserRole role)
        {
            if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException("El nombre es requerido.");
            if (string.IsNullOrWhiteSpace(keycloakId)) throw new ArgumentException("El ID de Keycloak es requerido.");

            FullName = fullName;
            Email = ValueObjects.Email.Create(email).Value;
            KeycloakId = keycloakId;
            Role = role;
            State = UserState.Active;

            AddHistory("UserCreated", $"Usuario creado con rol {role}");
        }

        public void UpdatePreferences(List<string> newPreferences)
        {
            if (newPreferences == null) 
                throw new ArgumentException("La lista de preferencias no puede ser nula.");

            // Detectar cambios para el historial
            var oldPrefs = string.Join(", ", _preferences ?? new List<string>());
            var newPrefs = string.Join(", ", newPreferences);

            if (oldPrefs != newPrefs)
            {
                _preferences = newPreferences; // Reemplazamos la lista
                UpdatedAt = DateTime.UtcNow;
                // AddHistory moved to Handler to avoid concurrency issues
            }
        }
    
        public void UpdateProfile(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName)) throw new ArgumentException("El nombre no puede estar vacío.");

            var oldName = FullName ?? "";
            FullName = newName ?? "";
            UpdatedAt = DateTime.UtcNow ;
        }

        private void AddHistory(string action, string details)
        {
            _history.Add(new UserHistory(this.Id, action, details));
        }

        public void Deactivate()
        {
            if (State == UserState.Inactive) return;
            
            State = UserState.Inactive;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}