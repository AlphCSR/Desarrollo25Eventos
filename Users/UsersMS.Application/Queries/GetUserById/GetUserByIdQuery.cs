using MediatR;
using System;
using System.Diagnostics.CodeAnalysis;
using UsersMS.Application.DTOs;

namespace UsersMS.Application.Queries.GetUserById
{
    [ExcludeFromCodeCoverage]
    public record GetUserByIdQuery(Guid UserId) : IRequest<UserDto?>;
}