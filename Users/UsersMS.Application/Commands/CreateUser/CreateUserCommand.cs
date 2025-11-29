using MediatR;
using System;
using UsersMS.Application.DTOs;

namespace UsersMS.Application.Commands.CreateUser
{
    public record CreateUserCommand(CreateUserDto UserData) : IRequest<Guid>;
}