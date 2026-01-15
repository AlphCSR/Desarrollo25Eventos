using MediatR;
using ServicesMS.Application.DTOs;
using ServicesMS.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ServicesMS.Application.Queries
{
    public class GetServicesByEventQueryHandler : IRequestHandler<GetServicesByEventQuery, IEnumerable<ServiceDefinitionDto>>
    {
        private readonly IServiceRepository _repository;

        public GetServicesByEventQueryHandler(IServiceRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<ServiceDefinitionDto>> Handle(GetServicesByEventQuery request, CancellationToken cancellationToken)
        {
            var services = await _repository.GetDefinitionsByEventAsync(request.EventId, cancellationToken);
            return services.Select(s => new ServiceDefinitionDto(s.Id, s.Name, s.Description, s.BasePrice, s.RequiresStock, s.Stock));
        }
    }
}
