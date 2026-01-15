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
                .ToListAsync();
        }

        public async Task<(IEnumerable<Booking> Items, int TotalCount)> GetPagedByUserIdAsync(Guid userId, int page, int pageSize)
        {
            var query = _context.Bookings.Where(x => x.UserId == userId);
            
            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<IEnumerable<Booking>> GetByEventIdAsync(Guid eventId)
        {
            return await _context.Bookings
                .Where(x => x.EventId == eventId)
                .ToListAsync();
        }

        public async Task<Booking?> GetActiveByEventAsync(Guid userId, Guid eventId, CancellationToken cancellationToken)
        {
            return await _context.Bookings
                .FirstOrDefaultAsync(x => x.UserId == userId && x.EventId == eventId && x.Status == Shared.Enums.BookingStatus.PendingPayment, cancellationToken);
        }

        public async Task<IEnumerable<Booking>> GetExpiredBookingsAsync(DateTime expirationTime, CancellationToken cancellationToken)
        {
            return await _context.Bookings
                .Where(x => x.Status == Shared.Enums.BookingStatus.PendingPayment && x.CreatedAt < expirationTime)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Booking>> GetBookingsNeedingReminderAsync(DateTime reminderThreshold, CancellationToken cancellationToken)
        {
            return await _context.Bookings
                .Where(x => x.Status == Shared.Enums.BookingStatus.PendingPayment 
                            && !x.PaymentReminderSent 
                            && x.CreatedAt < reminderThreshold)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Booking>> GetRecentlyCancelledBookingsAsync(DateTime since, CancellationToken cancellationToken)
        {
            return await _context.Bookings
                .Where(x => x.Status == Shared.Enums.BookingStatus.Cancelled 
                            && x.CancelledAt != null 
                            && x.CancelledAt >= since)
                .ToListAsync(cancellationToken);
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