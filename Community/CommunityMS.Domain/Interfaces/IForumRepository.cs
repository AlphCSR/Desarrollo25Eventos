using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CommunityMS.Domain.Entities;

namespace CommunityMS.Domain.Interfaces
{
    public interface IForumRepository
    {
        Task<IEnumerable<ForumThread>> GetThreadsByEventAsync(Guid eventId, CancellationToken cancellationToken);
        Task<ForumThread?> GetThreadByIdAsync(Guid threadId, CancellationToken cancellationToken);
        Task AddThreadAsync(ForumThread thread, CancellationToken cancellationToken);
        Task AddPostAsync(ForumPost post, CancellationToken cancellationToken);
        Task UpdateThreadAsync(ForumThread thread, CancellationToken cancellationToken);
        Task UpdatePostAsync(ForumPost post, CancellationToken cancellationToken);
        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}
