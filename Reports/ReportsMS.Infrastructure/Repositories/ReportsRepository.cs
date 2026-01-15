using Microsoft.EntityFrameworkCore;
using ReportsMS.Domain.Entities;
using ReportsMS.Domain.Interfaces;
using ReportsMS.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ReportsMS.Infrastructure.Repositories
{
    public class ReportsRepository : IReportsRepository
    {
        private readonly ReportsDbContext _context;

        public ReportsRepository(ReportsDbContext context)
        {
            _context = context;
        }

        public async Task AddSaleAsync(SalesRecord sale, CancellationToken cancellationToken)
        {
            await _context.SalesRecords.AddAsync(sale, cancellationToken);
        }

        public async Task<IEnumerable<SalesRecord>> GetSalesByEventAsync(Guid eventId, CancellationToken cancellationToken)
        {
            return await _context.SalesRecords
                .Where(x => x.EventId == eventId)
                .ToListAsync(cancellationToken);
        }

        public async Task AddEventStatsAsync(EventStats stats, CancellationToken cancellationToken)
        {
            await _context.EventStats.AddAsync(stats, cancellationToken);
        }

        public async Task<EventStats?> GetEventStatsAsync(Guid eventId, CancellationToken cancellationToken)
        {
            return await _context.EventStats.FirstOrDefaultAsync(x => x.EventId == eventId, cancellationToken);
        }

        public async Task UpdateEventStatsAsync(EventStats stats, CancellationToken cancellationToken)
        {
            _context.EventStats.Update(stats);
            await Task.CompletedTask;
        }

        public async Task UpdateMetricAsync(DashboardMetric metric, CancellationToken cancellationToken)
        {
            var existing = await _context.DashboardMetrics.FirstOrDefaultAsync(m => m.MetricName == metric.MetricName, cancellationToken);
            if (existing == null)
            {
                await _context.DashboardMetrics.AddAsync(metric, cancellationToken);
            }
            else
            {
                existing.UpdateValue(metric.Value);
                _context.DashboardMetrics.Update(existing);
            }
        }

        public async Task<DashboardMetric?> GetMetricByNameAsync(string metricName, CancellationToken cancellationToken)
        {
            return await _context.DashboardMetrics.FirstOrDefaultAsync(m => m.MetricName == metricName, cancellationToken);
        }

        public async Task<IEnumerable<DashboardMetric>> GetAllMetricsAsync(CancellationToken cancellationToken)
        {
            return await _context.DashboardMetrics.ToListAsync(cancellationToken);
        }

        public async Task<object> GetDailySalesAsync(CancellationToken cancellationToken)
        {
            return await _context.SalesRecords
                .GroupBy(s => s.Date.Date)
                .Select(g => new { Date = g.Key, Total = g.Sum(x => x.Amount), Count = g.Count() })
                .OrderByDescending(x => x.Date)
                .Take(30)
                .ToListAsync(cancellationToken);
        }

        public async Task<object> GetEventOccupancyAsync(CancellationToken cancellationToken)
        {
            return await _context.EventStats
                .Select(e => new { EventId = e.EventId, Capacity = e.TotalCapacity, Sold = e.SoldSeats, Occupancy = e.TotalCapacity > 0 ? (double)e.SoldSeats / e.TotalCapacity * 100 : 0 })
                .ToListAsync(cancellationToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
