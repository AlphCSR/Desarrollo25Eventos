using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReportsMS.Domain.Entities;
using ReportsMS.Domain.Interfaces;
using ReportsMS.Application.Interfaces;
using ReportsMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ReportsMS.Infrastructure.BackgroundJobs
{
    public class MetricsAggregationWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MetricsAggregationWorker> _logger;

        public MetricsAggregationWorker(IServiceProvider serviceProvider, ILogger<MetricsAggregationWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker de agregación de métricas iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<ReportsDbContext>();
                        var repo = scope.ServiceProvider.GetRequiredService<IReportsRepository>();
                        var auditService = scope.ServiceProvider.GetRequiredService<IAuditService>();

                        _logger.LogInformation("Calculando métricas...");

                        var totalRevenue = await context.SalesRecords.SumAsync(s => s.Amount, stoppingToken);
                        await repo.UpdateMetricAsync(new DashboardMetric("TotalRevenue", totalRevenue), stoppingToken);

                        var totalEvents = await context.EventStats.CountAsync(stoppingToken); 
                        await repo.UpdateMetricAsync(new DashboardMetric("TotalEvents", totalEvents), stoppingToken);

                        var totalTickets = await context.EventStats.SumAsync(e => e.SoldSeats, stoppingToken);
                        await repo.UpdateMetricAsync(new DashboardMetric("TotalTickets", totalTickets), stoppingToken);

                        var uniqueBuyers = await context.SalesRecords.Select(s => s.UserId).Distinct().CountAsync(stoppingToken);
                        await repo.UpdateMetricAsync(new DashboardMetric("TotalBuyers", uniqueBuyers), stoppingToken);

                        await repo.SaveChangesAsync(stoppingToken);
                        _logger.LogInformation("Métricas actualizadas con éxito.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ocurrió un error al calcular las métricas.");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
