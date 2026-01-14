using BookingMS.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BookingMS.Domain.Interfaces
{
    public interface IBookingRepository
    {
        Task AddAsync(Booking booking, CancellationToken cancellationToken);
        Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<IEnumerable<Booking>> GetByUserIdAsync(Guid userId);
        Task<(IEnumerable<Booking> Items, int TotalCount)> GetPagedByUserIdAsync(Guid userId, int page, int pageSize);
        Task<IEnumerable<Booking>> GetByEventIdAsync(Guid eventId);
        Task<Booking?> GetActiveByEventAsync(Guid userId, Guid eventId, CancellationToken cancellationToken);
        Task<IEnumerable<Booking>> GetExpiredBookingsAsync(DateTime expirationTime, CancellationToken cancellationToken);
        Task<IEnumerable<Booking>> GetBookingsNeedingReminderAsync(DateTime reminderThreshold, CancellationToken cancellationToken);
        Task<IEnumerable<Booking>> GetRecentlyCancelledBookingsAsync(DateTime since, CancellationToken cancellationToken);
        Task UpdateAsync(Booking booking);
        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}