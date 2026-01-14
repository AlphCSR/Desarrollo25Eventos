using MediatR;
using ServicesMS.Application.DTOs;
using ServicesMS.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace ServicesMS.Application.Queries
{
    public class GetServiceByIdQueryHandler : IRequestHandler<GetServiceByIdQuery, ServiceDefinitionDto>
    {
        private readonly IServiceRepository _repository;

        public GetServiceByIdQueryHandler(IServiceRepository repository)
        {
            _repository = repository;
        }

        public async Task<ServiceDefinitionDto> Handle(GetServiceByIdQuery request, CancellationToken cancellationToken)
        {
            var s = await _repository.GetDefinitionByIdAsync(request.Id, cancellationToken);
            if (s == null) return null;

            return new ServiceDefinitionDto(s.Id, s.Name, s.Description, s.BasePrice, s.RequiresStock, s.Stock);
        }
    }
}
