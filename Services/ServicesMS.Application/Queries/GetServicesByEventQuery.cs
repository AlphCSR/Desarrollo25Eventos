using MediatR;
using ServicesMS.Application.DTOs;
using System;
using System.Collections.Generic;

namespace ServicesMS.Application.Queries
{
    public record GetServicesByEventQuery(Guid EventId) : IRequest<IEnumerable<ServiceDefinitionDto>>;
}
