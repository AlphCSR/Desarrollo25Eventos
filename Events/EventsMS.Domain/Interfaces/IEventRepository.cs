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
        Task<IEnumerable<Event>> GetEventsToStartAsync(DateTime cutoff, CancellationToken cancellationToken);
        Task<IEnumerable<Event>> GetEventsToEndAsync(DateTime cutoff, CancellationToken cancellationToken);
        Task<IEnumerable<Event>> GetLiveEventsAsync(CancellationToken cancellationToken);
        Task<(IEnumerable<Event> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? category, DateTime? startDate, DateTime? endDate, decimal? minPrice, decimal? maxPrice, int? type, bool includeAll, bool futureOnly, CancellationToken cancellationToken);
        Task UpdateAsync(Event eventEntity, CancellationToken cancellationToken);
        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}
