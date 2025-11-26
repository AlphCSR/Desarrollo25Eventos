using System;
using UsersMS.Shared.Enums;

namespace UsersMS.Application.DTOs
{
    public record UserDto(Guid Id, string FullName, string Email, string KeycloakId, UserRole Role, UserState State);
}