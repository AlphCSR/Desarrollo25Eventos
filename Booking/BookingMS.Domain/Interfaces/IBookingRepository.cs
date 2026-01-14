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
        Task<Booking?> GetActiveByEventAsync(Guid userId, Guid eventId, CancellationToken cancellationToken);
        Task UpdateAsync(Booking booking);
        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}