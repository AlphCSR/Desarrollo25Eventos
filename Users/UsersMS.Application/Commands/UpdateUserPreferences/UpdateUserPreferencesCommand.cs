using MediatR;
using System;
using System.Collections.Generic;
using UsersMS.Application.DTOs;

namespace UsersMS.Application.Commands
{
    public record UpdateUserPreferencesCommand(Guid UserId, UpdateUserPreferencesDto Data) : IRequest;
}