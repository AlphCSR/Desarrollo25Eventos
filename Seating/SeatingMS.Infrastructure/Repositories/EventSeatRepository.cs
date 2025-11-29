using Microsoft.EntityFrameworkCore;
using SeatingMS.Domain.Interfaces;
using SeatingMS.Domain.Entities;
using SeatingMS.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SeatingMS.Infrastructure.Repositories
{
    public class EventSeatRepository : IEventSeatRepository
    {
        private readonly SeatingDbContext _context;

        public EventSeatRepository(SeatingDbContext context)
        {
            _context = context;
        }

        public async Task<EventSeat?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.EventSeats.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<EventSeat>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken)
        {
            return await _context.EventSeats
                .Where(x => x.EventId == eventId)
                .OrderBy(x => x.Row).ThenBy(x => x.Number)
                .ToListAsync(cancellationToken);
        }

        public async Task AddRangeAsync(IEnumerable<EventSeat> seats, CancellationToken cancellationToken)
        {
            await _context.EventSeats.AddRangeAsync(seats, cancellationToken);
        }

        public Task UpdateAsync(EventSeat seat, CancellationToken cancellationToken)
        {
            _context.EventSeats.Update(seat);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}