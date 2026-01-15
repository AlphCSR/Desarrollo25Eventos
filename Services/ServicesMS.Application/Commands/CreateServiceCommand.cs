using MediatR;
using ServicesMS.Application.DTOs;
using System;

namespace ServicesMS.Application.Commands
{
    public record CreateServiceCommand(CreateServiceDto Dto) : IRequest<Guid>;
}
