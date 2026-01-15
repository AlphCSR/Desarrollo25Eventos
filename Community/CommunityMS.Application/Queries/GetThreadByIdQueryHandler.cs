using System.Threading;
using System.Threading.Tasks;
using CommunityMS.Domain.Entities;
using CommunityMS.Domain.Interfaces;
using MediatR;

namespace CommunityMS.Application.Queries
{
    public class GetThreadByIdQueryHandler : IRequestHandler<GetThreadByIdQuery, ForumThread>
    {
        private readonly IForumRepository _repository;

        public GetThreadByIdQueryHandler(IForumRepository repository)
        {
            _repository = repository;
        }

        public async Task<ForumThread> Handle(GetThreadByIdQuery request, CancellationToken cancellationToken)
        {
            var thread = await _repository.GetThreadByIdAsync(request.ThreadId, cancellationToken);
            if (thread == null)
            {
                throw new System.Exception("Thread not found");
            }
            return thread;
        }
    }
}
