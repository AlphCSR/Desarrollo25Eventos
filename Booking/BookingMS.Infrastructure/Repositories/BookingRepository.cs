using Microsoft.EntityFrameworkCore;
using BookingMS.Domain.Interfaces;
using BookingMS.Domain.Entities;
using BookingMS.Infrastructure.Persistence.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BookingMS.Infrastructure.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly BookingDbContext _context;

        public BookingRepository(BookingDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Booking booking, CancellationToken cancellationToken)
        {
            await _context.Bookings.AddAsync(booking, cancellationToken);
        }

        public async Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Bookings.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<Booking>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Bookings
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<Booking?> GetActiveByEventAsync(Guid userId, Guid eventId, CancellationToken cancellationToken)
        {
            return await _context.Bookings
                .FirstOrDefaultAsync(x => x.UserId == userId && x.EventId == eventId && x.Status == Shared.Enums.BookingStatus.PendingPayment, cancellationToken);
        }

        public Task UpdateAsync(Booking booking)
        {
            _context.Bookings.Update(booking);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}