using BookingMS.Shared.Events;
using MassTransit;
using ReportsMS.Domain.Entities;
using ReportsMS.Domain.Interfaces;
using System.Threading.Tasks;

namespace ReportsMS.Infrastructure.Consumers
{
    public class SalesRecordingConsumer : IConsumer<BookingConfirmedEvent>
    {
        private readonly IReportsRepository _repository;

        public SalesRecordingConsumer(IReportsRepository repository)
        {
            _repository = repository;
        }

        public async Task Consume(ConsumeContext<BookingConfirmedEvent> context)
        {
            var message = context.Message;
            
            var sale = new SalesRecord(message.EventId, message.BookingId, message.UserId, message.Email, message.TotalAmount, message.ConfirmedAt);
            
            await _repository.AddSaleAsync(sale, context.CancellationToken);

            var revenueMetric = await _repository.GetMetricByNameAsync("TotalRevenue", context.CancellationToken);
            if (revenueMetric == null)
            {
                await _repository.UpdateMetricAsync(new DashboardMetric("TotalRevenue", message.TotalAmount), context.CancellationToken);
            }
            else
            {
                revenueMetric.UpdateValue(revenueMetric.Value + message.TotalAmount);
                await _repository.UpdateMetricAsync(revenueMetric, context.CancellationToken);
            }

            var ticketsCount = message.SeatIds?.Count ?? 1; 
            var ticketsMetric = await _repository.GetMetricByNameAsync("TotalTickets", context.CancellationToken);
            if (ticketsMetric == null)
            {
                await _repository.UpdateMetricAsync(new DashboardMetric("TotalTickets", ticketsCount), context.CancellationToken);
            }
            else
            {
                ticketsMetric.UpdateValue(ticketsMetric.Value + ticketsCount);
                await _repository.UpdateMetricAsync(ticketsMetric, context.CancellationToken);
            }

            var buyersMetric = await _repository.GetMetricByNameAsync("TotalBuyers", context.CancellationToken);
            if (buyersMetric == null)
            {
                await _repository.UpdateMetricAsync(new DashboardMetric("TotalBuyers", 1), context.CancellationToken);
            }
            else
            {
                buyersMetric.UpdateValue(buyersMetric.Value + 1);
                await _repository.UpdateMetricAsync(buyersMetric, context.CancellationToken);
            }

            var stats = await _repository.GetEventStatsAsync(message.EventId, context.CancellationToken);
            if (stats != null)
            {
                var count = message.SeatIds?.Count ?? 0;
                stats.IncrementSoldSeats(count > 0 ? count : 1); 
                await _repository.UpdateEventStatsAsync(stats, context.CancellationToken);
            }

            await _repository.SaveChangesAsync(context.CancellationToken);
        }
    }
}
