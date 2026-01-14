using System.Diagnostics.CodeAnalysis;
using MediatR;
using UsersMS.Application.DTOs;

namespace UsersMS.Application.Queries.GetUserByEmail
{
    [ExcludeFromCodeCoverage]
    public record GetUserByEmailQuery(string Email) : IRequest<UserDto?>;
}