using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityMS.Domain.Entities;
using CommunityMS.Domain.Interfaces;
using MediatR;

namespace CommunityMS.Application.Commands
{
    public class AddPostCommandHandler : IRequestHandler<AddPostCommand, Guid>
    {
        private readonly IForumRepository _repository;

        public AddPostCommandHandler(IForumRepository repository)
        {
            _repository = repository;
        }

        public async Task<Guid> Handle(AddPostCommand request, CancellationToken cancellationToken)
        {
            var thread = await _repository.GetThreadByIdAsync(request.ThreadId, cancellationToken);
            if (thread == null)
            {
                throw new Exception("Thread not found");
            }

            var post = new ForumPost(
                request.Content,
                request.ThreadId,
                request.AuthorId,
                request.AuthorName
            );

            thread.AddPost(post);
            await _repository.AddPostAsync(post, cancellationToken);
            await _repository.UpdateThreadAsync(thread, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            return post.Id;
        }
    }
}
