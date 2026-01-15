using EventsMS.Shared.Events;
using MassTransit;
using ReportsMS.Domain.Entities;
using ReportsMS.Domain.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace ReportsMS.Infrastructure.Consumers
{
    public class EventCreatedConsumer : IConsumer<EventCreatedEvent>
    {
        private readonly IReportsRepository _repository;

        public EventCreatedConsumer(IReportsRepository repository)
        {
            _repository = repository;
        }

        public async Task Consume(ConsumeContext<EventCreatedEvent> context)
        {
            var message = context.Message;
            
            var totalCapacity = message.Sections.Sum(s => s.Capacity);
            
            var stats = new EventStats(message.EventId, totalCapacity);
            
            await _repository.AddEventStatsAsync(stats, context.CancellationToken);

            var totalEventsMetric = await _repository.GetMetricByNameAsync("TotalEvents", context.CancellationToken);
            if (totalEventsMetric == null)
            {
                await _repository.UpdateMetricAsync(new DashboardMetric("TotalEvents", 1), context.CancellationToken);
            }
            else
            {
                totalEventsMetric.UpdateValue(totalEventsMetric.Value + 1);
                await _repository.UpdateMetricAsync(totalEventsMetric, context.CancellationToken);
            }

            await _repository.SaveChangesAsync(context.CancellationToken);
        }
    }
}
