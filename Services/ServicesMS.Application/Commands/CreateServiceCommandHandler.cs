using MediatR;
using ServicesMS.Domain.Entities;
using ServicesMS.Domain.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServicesMS.Application.Commands
{
    public class CreateServiceCommandHandler : IRequestHandler<CreateServiceCommand, Guid>
    {
        private readonly IServiceRepository _repository;

        public CreateServiceCommandHandler(IServiceRepository repository)
        {
            _repository = repository;
        }

        public async Task<Guid> Handle(CreateServiceCommand request, CancellationToken cancellationToken)
        {
            var service = new ServiceDefinition(
                request.Dto.Name,
                request.Dto.Description,
                request.Dto.BasePrice,
                request.Dto.RequiresStock,
                request.Dto.Stock,
                request.Dto.EventId
            );

            await _repository.AddDefinitionAsync(service, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            return service.Id;
        }
    }
}
