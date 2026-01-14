using EventsMS.Domain.Interfaces;
using EventsMS.Shared.Events;
using Microsoft.Extensions.Logging;
using MassTransit;
using System.Diagnostics.CodeAnalysis;

namespace EventsMS.Infrastructure.BackgroundJobs
{
    [ExcludeFromCodeCoverage]
    public class EventJobs
    {
        private readonly IEventRepository _repository;
        private readonly IPublishEndpoint _publisher;
        private readonly ILogger<EventJobs> _logger;

        public EventJobs(IEventRepository repository, IPublishEndpoint publisher, ILogger<EventJobs> logger)
        {
            _repository = repository;
            _publisher = publisher;
            _logger = logger;
        }

        public async Task ProcessEventTransitionsAsync()
        {
            _logger.LogInformation("Hangfire: Procesando transiciones de eventos...");
            
            var now = DateTime.UtcNow;

            var toStart = await _repository.GetEventsToStartAsync(now, CancellationToken.None);
            foreach (var evt in toStart)
            {
                if (evt.EndDate < now) continue; 

                _logger.LogInformation($"Hangfire: Evento {evt.Id} comenzando. Cambiando a LIVE.");
                evt.Start();
                await _repository.UpdateAsync(evt, CancellationToken.None);
                await _publisher.Publish(new EventStatusChangedEvent 
                { 
                    EventId = evt.Id, 
                    NewStatus = evt.Status,
                    Sections = evt.Sections.Select(s => new SectionDto(s.Id, s.Name, s.Price, s.Capacity, s.IsNumbered)).ToList()
                });
            }

            var toFinish = await _repository.GetEventsToEndAsync(now, CancellationToken.None);
            foreach (var evt in toFinish)
            {
                _logger.LogInformation($"Hangfire: Evento {evt.Id} terminando. Cambiando a FINISHED.");
                evt.Finish();
                await _repository.UpdateAsync(evt, CancellationToken.None);
                await _publisher.Publish(new EventStatusChangedEvent 
                { 
                    EventId = evt.Id, 
                    NewStatus = evt.Status,
                    Sections = evt.Sections.Select(s => new SectionDto(s.Id, s.Name, s.Price, s.Capacity, s.IsNumbered)).ToList()
                });
            }

            if (toStart.Any() || toFinish.Any())
            {
                await _repository.SaveChangesAsync(CancellationToken.None);
            }
        }
    }
}
