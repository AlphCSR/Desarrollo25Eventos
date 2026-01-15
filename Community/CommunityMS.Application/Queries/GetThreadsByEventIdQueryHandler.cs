using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CommunityMS.Domain.Entities;
using CommunityMS.Domain.Interfaces;
using MediatR;

namespace CommunityMS.Application.Queries
{
    public class GetThreadsByEventIdQueryHandler : IRequestHandler<GetThreadsByEventIdQuery, IEnumerable<ForumThread>>
    {
        private readonly IForumRepository _repository;

        public GetThreadsByEventIdQueryHandler(IForumRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<ForumThread>> Handle(GetThreadsByEventIdQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetThreadsByEventAsync(request.EventId, cancellationToken);
        }
    }
}
