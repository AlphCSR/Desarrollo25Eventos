using System;
using System.Diagnostics.CodeAnalysis;
using UsersMS.Shared.Enums;
using System.Collections.Generic; // Added for List<string>

namespace UsersMS.Application.DTOs
{
    [ExcludeFromCodeCoverage]
    public record UserDto(Guid Id, string FullName, string Email, string KeycloakId, UserRole Role, UserState State)
    {
        public List<string> Preferences { get; init; } = new();
    }
}