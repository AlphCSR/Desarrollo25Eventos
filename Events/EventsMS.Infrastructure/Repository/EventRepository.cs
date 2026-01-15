using Microsoft.EntityFrameworkCore;
using EventsMS.Domain.Interfaces;
using EventsMS.Domain.Entities;
using EventsMS.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventsMS.Shared.Enums;

namespace EventsMS.Infrastructure.Repository
{
    public class EventRepository : IEventRepository
    {
        private readonly EventsDbContext _context;

        public EventRepository(EventsDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Event eventEntity, CancellationToken cancellationToken)
        {
            await _context.Events.AddAsync(eventEntity, cancellationToken);
        }

        public async Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Events
                .Include(e => e.Sections)
                .ThenInclude(s => s.Seats)
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<Event>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _context.Events
                .Include(e => e.Sections) 
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Event>> GetEventsToStartAsync(DateTime cutoff, CancellationToken cancellationToken)
        {
            return await _context.Events
                .Include(e => e.Sections)
                .Where(e => e.Status == EventStatus.Published && e.Date <= cutoff)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Event>> GetEventsToEndAsync(DateTime cutoff, CancellationToken cancellationToken)
        {
            return await _context.Events
                .Include(e => e.Sections)
                .Where(e => (e.Status == EventStatus.Published || e.Status == EventStatus.Live) 
                            && e.EndDate < cutoff)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Event>> GetLiveEventsAsync(CancellationToken cancellationToken)
        {
            return await _context.Events
                .Where(e => e.Status == EventStatus.Live)
                .ToListAsync(cancellationToken);
        }

        public async Task<(IEnumerable<Event> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? category, DateTime? startDate, DateTime? endDate, decimal? minPrice, decimal? maxPrice, int? type, bool includeAll, bool futureOnly, CancellationToken cancellationToken)
        {
            var query = _context.Events.AsQueryable();

            if (!includeAll)
            {
                query = query.Where(e => e.Status == EventStatus.Published || e.Status == EventStatus.Live);
            }

            if (futureOnly)
            {
                var now = DateTime.UtcNow;
                query = query.Where(e => e.Date >= now || e.Status == EventStatus.Live);
            }

            if (!string.IsNullOrEmpty(category))
            {
                var cat = category.ToLower();
                query = query.Where(e => e.Categories != null && e.Categories.Any(c => c.ToLower().Contains(cat)));
            }

            if (startDate.HasValue)
                query = query.Where(e => e.Date >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(e => e.EndDate <= endDate.Value);

            if (minPrice.HasValue)
                query = query.Where(e => e.Sections.Any(s => s.Price >= minPrice.Value));

            if (maxPrice.HasValue)
                query = query.Where(e => e.Sections.Any(s => s.Price <= maxPrice.Value));

            if (type.HasValue)
                query = query.Where(e => (int)e.Type == type.Value);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(e => e.Date)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task UpdateAsync(Event eventEntity, CancellationToken cancellationToken)
        {
            _context.Events.Update(eventEntity);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
