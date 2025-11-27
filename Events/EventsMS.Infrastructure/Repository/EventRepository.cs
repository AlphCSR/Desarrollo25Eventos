using Microsoft.EntityFrameworkCore;
using EventsMS.Domain.Interfaces;
using EventsMS.Domain.Entities;
using EventsMS.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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
            // Para listas, mejor no traer todos los asientos (performance)
            return await _context.Events
                .Include(e => e.Sections) 
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}