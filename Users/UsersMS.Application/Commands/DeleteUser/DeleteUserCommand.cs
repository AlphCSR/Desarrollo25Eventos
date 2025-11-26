using MediatR;
using System;

namespace UsersMS.Application.Commands.DeleteUser
{
    public record DeleteUserCommand(Guid Id) : IRequest;
}