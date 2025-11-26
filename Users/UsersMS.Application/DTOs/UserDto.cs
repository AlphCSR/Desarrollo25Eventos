using System;
using System.Diagnostics.CodeAnalysis;
using UsersMS.Shared.Enums;

namespace UsersMS.Application.DTOs
{
    [ExcludeFromCodeCoverage]
    public record UserDto(Guid Id, string FullName, string Email, string KeycloakId, UserRole Role, UserState State);
}