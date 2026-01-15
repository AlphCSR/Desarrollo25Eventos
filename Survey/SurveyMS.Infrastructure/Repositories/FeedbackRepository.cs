
using Microsoft.EntityFrameworkCore;
using SurveyMS.Domain.Entities;
using SurveyMS.Domain.Interfaces;
using SurveyMS.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SurveyMS.Infrastructure.Repositories
{
    public class FeedbackRepository : IFeedbackRepository
    {
        private readonly SurveyDbContext _context;

        public FeedbackRepository(SurveyDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Feedback feedback, CancellationToken cancellationToken = default)
        {
            await _context.Feedbacks.AddAsync(feedback, cancellationToken);
        }

        public async Task<IEnumerable<Feedback>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
        {
            return await _context.Feedbacks
                .Where(f => f.EventId == eventId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> HasUserRatedBookingAsync(Guid userId, Guid bookingId, CancellationToken cancellationToken = default)
        {
            return await _context.Feedbacks.AnyAsync(f => f.UserId == userId && f.BookingId == bookingId, cancellationToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
