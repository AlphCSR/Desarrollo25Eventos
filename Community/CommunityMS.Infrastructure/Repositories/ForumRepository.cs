using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityMS.Domain.Entities;
using CommunityMS.Domain.Interfaces;
using CommunityMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CommunityMS.Infrastructure.Repositories
{
    public class ForumRepository : IForumRepository
    {
        private readonly CommunityDbContext _context;

        public ForumRepository(CommunityDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ForumThread>> GetThreadsByEventAsync(Guid eventId, CancellationToken cancellationToken)
        {
            return await _context.Threads
                .Where(t => t.EventId == eventId)
                .OrderByDescending(t => t.IsPinned)
                .ThenByDescending(t => t.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<ForumThread?> GetThreadByIdAsync(Guid threadId, CancellationToken cancellationToken)
        {
            return await _context.Threads
                .Include(t => t.Posts.OrderBy(p => p.CreatedAt))
                .FirstOrDefaultAsync(t => t.Id == threadId, cancellationToken);
        }

        public async Task AddThreadAsync(ForumThread thread, CancellationToken cancellationToken)
        {
            await _context.Threads.AddAsync(thread, cancellationToken);
        }

        public async Task AddPostAsync(ForumPost post, CancellationToken cancellationToken)
        {
            await _context.Posts.AddAsync(post, cancellationToken);
        }

        public Task UpdateThreadAsync(ForumThread thread, CancellationToken cancellationToken)
        {
            _context.Threads.Update(thread);
            return Task.CompletedTask;
        }

         public Task UpdatePostAsync(ForumPost post, CancellationToken cancellationToken)
        {
            _context.Posts.Update(post);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
