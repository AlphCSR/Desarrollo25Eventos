using System.Threading;
using System.Threading.Tasks;
using CommunityMS.Domain.Entities;
using CommunityMS.Domain.Interfaces;
using MediatR;

namespace CommunityMS.Application.Commands
{
    public class CreateThreadCommandHandler : IRequestHandler<CreateThreadCommand, Guid>
    {
        private readonly IForumRepository _repository;

        public CreateThreadCommandHandler(IForumRepository repository)
        {
            _repository = repository;
        }

        public async Task<Guid> Handle(CreateThreadCommand request, CancellationToken cancellationToken)
        {
            var thread = new ForumThread(
                request.Title,
                request.Content,
                request.EventId,
                request.AuthorId,
                request.AuthorName
            );

            await _repository.AddThreadAsync(thread, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            return thread.Id;
        }
    }
}
