using UsersMS.Shared.Enums;

namespace UsersMS.Application.DTOs
{
    public record CreateUserDto(string FullName, string Email, string Password, UserRole Role);
}