using System.Diagnostics.CodeAnalysis;
using UsersMS.Shared.Enums;

namespace UsersMS.Application.DTOs
{
    [ExcludeFromCodeCoverage]
    public record CreateUserDto(string FullName, string Email, string Password, UserRole Role);
}