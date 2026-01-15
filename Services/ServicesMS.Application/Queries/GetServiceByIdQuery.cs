using MediatR;
using ServicesMS.Application.DTOs;
using System;

namespace ServicesMS.Application.Queries
{
    public record GetServiceByIdQuery(Guid Id) : IRequest<ServiceDefinitionDto>;
}
