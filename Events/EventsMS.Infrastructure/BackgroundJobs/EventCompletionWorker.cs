using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using EventsMS.Domain.Interfaces;
using EventsMS.Shared.Events;
using EventsMS.Shared.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using MassTransit;

namespace EventsMS.Infrastructure.BackgroundJobs
{
    public class EventCompletionWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EventCompletionWorker> _logger;

        public EventCompletionWorker(IServiceProvider serviceProvider, ILogger<EventCompletionWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker de ciclo de vida de eventos iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessEventTransitions(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error procesando transiciones de ciclo de vida de eventos.");
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        private async Task ProcessEventTransitions(CancellationToken stoppingToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var repository = scope.ServiceProvider.GetRequiredService<IEventRepository>();
                var publisher = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

                var now = DateTime.UtcNow;

                var liveEvents = await repository.GetLiveEventsAsync(stoppingToken); 
                foreach (var evt in liveEvents)
                {
                    if (evt.Date > now.AddMinutes(1)) 
                    {
                        _logger.LogWarning($"El evento {evt.Id} ('{evt.Title}') está EN VIVO pero comienza en el futuro ({evt.Date}). Revirtiendo a PUBLICADO.");
                        evt.ResetToPublished(); 
                        await repository.UpdateAsync(evt, stoppingToken);
                        await publisher.Publish(new EventStatusChangedEvent 
                        { 
                            EventId = evt.Id, 
                            NewStatus = evt.Status,
                            Sections = evt.Sections.Select(s => new SectionDto(s.Id, s.Name, s.Price, s.Capacity, s.IsNumbered)).ToList()
                        }, stoppingToken);
                    }
                }
                
                var toStart = await repository.GetEventsToStartAsync(now, stoppingToken);
                foreach (var evt in toStart)
                {
                    if (evt.EndDate < now)
                    {
                        _logger.LogWarning($"El evento {evt.Id} ('{evt.Title}') pasó sin entrar en vivo (EndDate {evt.EndDate} < Now {now}).");
                        continue; 
                    }

                    _logger.LogInformation($"El evento {evt.Id} ('{evt.Title}') comenzando. Fecha: {evt.Date}, Ahora: {now}. Cambiando a EN VIVO.");
                    evt.Start();
                    await repository.UpdateAsync(evt, stoppingToken);
                    await publisher.Publish(new EventStatusChangedEvent 
                    { 
                        EventId = evt.Id, 
                        NewStatus = evt.Status,
                        Sections = evt.Sections.Select(s => new SectionDto(s.Id, s.Name, s.Price, s.Capacity, s.IsNumbered)).ToList()
                    }, stoppingToken);
                }

                var toFinish = await repository.GetEventsToEndAsync(now, stoppingToken);
                foreach (var evt in toFinish)
                {
                    _logger.LogInformation($"El evento {evt.Id} ('{evt.Title}') finalizando. Fecha de fin: {evt.EndDate}, Ahora: {now}. Cambiando a FINALIZADO.");
                    evt.Finish();
                    await repository.UpdateAsync(evt, stoppingToken);
                    await publisher.Publish(new EventStatusChangedEvent 
                    { 
                        EventId = evt.Id, 
                        NewStatus = evt.Status,
                        Sections = evt.Sections.Select(s => new SectionDto(s.Id, s.Name, s.Price, s.Capacity, s.IsNumbered)).ToList()
                    }, stoppingToken);
                }

                if (toStart.Any() || toFinish.Any() || liveEvents.Any(e => e.Status == EventStatus.Published))
                {
                    await repository.SaveChangesAsync(stoppingToken);
                }
            }
        }
    }
}
