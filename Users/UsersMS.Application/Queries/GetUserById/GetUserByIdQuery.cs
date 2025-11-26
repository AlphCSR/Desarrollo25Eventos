using MediatR;
using System;
using UsersMS.Application.DTOs;

namespace UsersMS.Application.Queries.GetUserById
{
    public record GetUserByIdQuery(Guid Id) : IRequest<UserDto?>;
}