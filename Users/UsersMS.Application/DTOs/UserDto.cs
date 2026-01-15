using System;
using System.Diagnostics.CodeAnalysis;
using UsersMS.Shared.Enums;
using System.Collections.Generic;

namespace UsersMS.Application.DTOs
{
    [ExcludeFromCodeCoverage]
    public record UserDto(
        Guid Id, 
        string FullName, 
        string Email, 
        string KeycloakId, 
        UserRole Role, 
        UserState State, 
        string? PhoneNumber, 
        string? DocumentId, 
        DateTime? DateOfBirth, 
        string? Address, 
        string? ProfilePictureUrl,
        List<string> Preferences,
        string Language)
    {
    }
}