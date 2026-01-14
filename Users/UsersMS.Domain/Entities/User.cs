using System;
using System.Collections.Generic;
using UsersMS.Domain.Common;
using UsersMS.Domain.ValueObjects;
using UsersMS.Shared.Enums;
using UsersMS.Domain.Exceptions;

namespace UsersMS.Domain.Entities
{
    public class User : BaseEntity
    {
        public PersonName FullName { get; private set; }
        public Email Email { get; private set; }
        public string KeycloakId { get; private set; }
        public UserRole Role { get; private set; }
        public UserState State { get; private set; }
        public List<string> Preferences { get; private set; } = new();

        public PhoneNumber? PhoneNumber { get; private set; }
        public string? DocumentId { get; private set; }
        public DateTime? DateOfBirth { get; private set; }
        public string? Address { get; private set; }
        public string? ProfilePictureUrl { get; private set; }
        public string Language { get; private set; } = "es";

        private readonly List<UserHistory> _history = new();
        
        public IReadOnlyCollection<UserHistory> History => _history.AsReadOnly();

        protected User() 
        { 
            FullName = null!;
            Email = null!;
            KeycloakId = null!;
            Language = "es";
        }

        public User(string fullName, string email, string keycloakId, UserRole role)
        {
            if (string.IsNullOrWhiteSpace(keycloakId)) throw new InvalidUserDataException("El ID de Keycloak es requerido.");

            FullName = PersonName.Create(fullName);
            Email = ValueObjects.Email.Create(email);
            KeycloakId = keycloakId;
            Role = role;
            State = UserState.Active;
        }

        public void UpdatePreferences(List<string> newPreferences)
        {
            if (newPreferences == null) 
                throw new InvalidUserDataException("La lista de preferencias no puede ser nula.");

            var oldPrefs = string.Join(", ", Preferences ?? new List<string>());
            var newPrefs = string.Join(", ", newPreferences);

            if (oldPrefs != newPrefs)
            {
                Preferences = newPreferences; 
                UpdatedAt = DateTime.UtcNow;
            }
        }
    
        public void UpdateProfile(string newName, string? phoneNumber, string? documentId, DateTime? dateOfBirth, string? address, string? profilePictureUrl, string? language = "es")
        {
            FullName = PersonName.Create(newName);
            PhoneNumber = !string.IsNullOrWhiteSpace(phoneNumber) ? ValueObjects.PhoneNumber.Create(phoneNumber) : null;
            DocumentId = documentId;
            DateOfBirth = dateOfBirth;
            Address = address;
            ProfilePictureUrl = profilePictureUrl;
            Language = language ?? "es";

            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            if (State == UserState.Inactive) return;
            
            State = UserState.Inactive;
            UpdatedAt = DateTime.UtcNow;
        }

    }
}
