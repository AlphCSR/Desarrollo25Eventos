using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SeatingMS.Domain.Interfaces;
using SeatingMS.Shared.Events;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using MassTransit;

namespace SeatingMS.Infrastructure.BackgroundJobs
{
    public class SeatLockExpirationWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SeatLockExpirationWorker> _logger;

        public SeatLockExpirationWorker(IServiceProvider serviceProvider, ILogger<SeatLockExpirationWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker de expiración de bloqueos de asientos iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessExpiredLocks(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error procesando expiración de bloqueos de asientos");
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        private async Task ProcessExpiredLocks(CancellationToken stoppingToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var repository = scope.ServiceProvider.GetRequiredService<IEventSeatRepository>();

                var now = DateTime.UtcNow;
                var expiredSeats = await repository.GetExpiredSeatsAsync(now, stoppingToken);

                if (expiredSeats != null && expiredSeats.Any())
                {
                    _logger.LogInformation($"Encontrados {expiredSeats.Count()} bloqueos de asientos expirados. Liberando...");

                    foreach (var seat in expiredSeats)
                    {
                        seat.Release();
                        await repository.UpdateAsync(seat, stoppingToken);

                        var publisher = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
                        await publisher.Publish(new SeatStatusUpdatedEvent
                        {
                            SeatId = seat.Id,
                            EventId = seat.EventId,
                            Status = seat.Status,
                            UserId = null
                        }, stoppingToken);
                    }
                    
                    await repository.SaveChangesAsync(stoppingToken);
                }
            }
        }
    }
}
