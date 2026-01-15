using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ReportsMS.Domain.Entities;

namespace ReportsMS.Domain.Interfaces
{
    public interface IReportsRepository
    {
        Task AddSaleAsync(SalesRecord sale, CancellationToken cancellationToken);
        Task<IEnumerable<SalesRecord>> GetSalesByEventAsync(Guid eventId, CancellationToken cancellationToken);
        
        Task AddEventStatsAsync(EventStats stats, CancellationToken cancellationToken);
        Task<EventStats?> GetEventStatsAsync(Guid eventId, CancellationToken cancellationToken);
        Task UpdateEventStatsAsync(EventStats stats, CancellationToken cancellationToken);

        Task UpdateMetricAsync(DashboardMetric metric, CancellationToken cancellationToken);
        Task<DashboardMetric?> GetMetricByNameAsync(string metricName, CancellationToken cancellationToken);
        Task<IEnumerable<DashboardMetric>> GetAllMetricsAsync(CancellationToken cancellationToken);

        Task<object> GetDailySalesAsync(CancellationToken cancellationToken);
        Task<object> GetEventOccupancyAsync(CancellationToken cancellationToken);

        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}
