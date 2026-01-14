using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EventsMS.Domain.Entities;

namespace EventsMS.Domain.Interfaces
{
    public interface IEventRepository
    {
        Task AddAsync(Event eventEntity, CancellationToken cancellationToken);
        Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<IEnumerable<Event>> GetAllAsync(CancellationToken cancellationToken);
        Task UpdateAsync(Event eventEntity, CancellationToken cancellationToken);
        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}
