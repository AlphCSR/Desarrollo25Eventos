using MediatR;
using System.Collections.Generic;
using UsersMS.Application.DTOs;

namespace UsersMS.Application.Queries.GetAllUsers
{
    public record GetAllUsersQuery() : IRequest<IEnumerable<UserDto>>;
}