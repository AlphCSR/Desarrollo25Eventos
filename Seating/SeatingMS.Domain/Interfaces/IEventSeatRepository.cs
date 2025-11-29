using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SeatingMS.Domain.Entities;

namespace SeatingMS.Domain.Interfaces
{
    public interface IEventSeatRepository
    {
        Task<EventSeat?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<IEnumerable<EventSeat>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken);
        Task AddRangeAsync(IEnumerable<EventSeat> seats, CancellationToken cancellationToken);
        Task UpdateAsync(EventSeat seat, CancellationToken cancellationToken);
        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}