using MediatR;
using EventsMS.Domain.Interfaces;
using EventsMS.Shared.Enums;
using System.Threading;
using System.Threading.Tasks;

namespace EventsMS.Application.Queries.GetStreamingLink
{
    public class GetStreamingLinkQueryHandler : IRequestHandler<GetStreamingLinkQuery, string>
    {
        private readonly IEventRepository _repository;

        public GetStreamingLinkQueryHandler(IEventRepository repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(GetStreamingLinkQuery request, CancellationToken cancellationToken)
        {
            var evt = await _repository.GetByIdAsync(request.EventId, cancellationToken);
            if (evt == null) return string.Empty;

            if (evt.Type == EventType.Physical) throw new InvalidOperationException("This event is physical and has no streaming link.");

            return evt.StreamingUrl ?? string.Empty;
        }
    }
}
