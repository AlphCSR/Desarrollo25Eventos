using System.Diagnostics.CodeAnalysis;
using MediatR;
using System.Collections.Generic;
using UsersMS.Application.DTOs;

namespace UsersMS.Application.Queries.GetAllUsers
{
    [ExcludeFromCodeCoverage]
    public record GetAllUsersQuery : IRequest<IEnumerable<UserDto>>;
}