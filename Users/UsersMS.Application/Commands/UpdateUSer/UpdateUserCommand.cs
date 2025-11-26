using MediatR;
using System;
using UsersMS.Application.DTOs;

namespace UsersMS.Application.Commands.UpdateUser
{
    public record UpdateUserCommand(Guid Id, UpdateUserDto Data) : IRequest;
}