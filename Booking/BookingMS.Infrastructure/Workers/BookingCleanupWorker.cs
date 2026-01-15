using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BookingMS.Domain.Interfaces;
using BookingMS.Shared.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BookingMS.Infrastructure.Workers
{
    public class BookingCleanupWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BookingCleanupWorker> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(2); // Check every 2 minutes
        private readonly TimeSpan _lookbackWindow = TimeSpan.FromMinutes(5); // Look back 5 minutes

        public BookingCleanupWorker(IServiceProvider serviceProvider, ILogger<BookingCleanupWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Limpieza de Reservas Worker ejecutándose.");

            using var timer = new PeriodicTimer(_checkInterval);

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await ProcessRecentlyCancelledBookings(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error ocurrido ejecutando Limpieza de Reservas Worker.");
                }
            }
        }

        private async Task ProcessRecentlyCancelledBookings(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
            var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

            var since = DateTime.UtcNow.Subtract(_lookbackWindow);
            _logger.LogInformation("Checking for bookings cancelled since {Since}", since);

            var cancelledBookings = await repository.GetRecentlyCancelledBookingsAsync(since, stoppingToken);

            if (!cancelledBookings.Any())
            {
                _logger.LogInformation("No reservas canceladas recientes encontradas.");
                return;
            }

            _logger.LogInformation("Encontradas {Count} reservas canceladas. Re-publicando eventos para garantizar consistencia.", cancelledBookings.Count());

            foreach (var booking in cancelledBookings)
            {
                try
                {
                    // Re-publish the event. This is idempotent for SeatingMS (it just unlocks again)
                    var cancelEvent = new BookingCancelledEvent
                    {
                        BookingId = booking.Id,
                        UserId = booking.UserId,
                        SeatIds = booking.SeatIds.ToList(),
                        Reason = "Reconciliation Job - Consistencia Check", // Indicate source
                        Email = booking.Email,
                        Language = "ES", // Default or stored?
                        EventName = "Bumping Cancellation" 
                    };

                    await publisher.PublishAsync(cancelEvent, stoppingToken);
                    _logger.LogInformation("Re-publicado BookingCancelledEvent para Booking {BookingId}", booking.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al re-publicar cancelación para Booking {BookingId}", booking.Id);
                }
            }
        }
    }
}
