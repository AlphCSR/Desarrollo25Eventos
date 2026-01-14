using MediatR;
using EventsMS.Domain.Interfaces;
using EventsMS.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace EventsMS.Application.Commands.UploadImage
{
    public class UploadEventImageCommandHandler : IRequestHandler<UploadEventImageCommand, string>
    {
        private readonly IEventRepository _repository;
        private readonly IFileStorageService _fileStorage;

        public UploadEventImageCommandHandler(IEventRepository repository, IFileStorageService fileStorage)
        {
            _repository = repository;
            _fileStorage = fileStorage;
        }

        public async Task<string> Handle(UploadEventImageCommand request, CancellationToken cancellationToken)
        {
            var eventEntity = await _repository.GetByIdAsync(request.EventId, cancellationToken);
            if (eventEntity == null) throw new Exception($"Evento {request.EventId} no encontrado");

            var imageUrl = await _fileStorage.SaveFileAsync("events", request.FileStream, request.FileName);

            eventEntity.SetImageUrl(imageUrl);

            await _repository.UpdateAsync(eventEntity, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            return imageUrl;
        }
    }
}
