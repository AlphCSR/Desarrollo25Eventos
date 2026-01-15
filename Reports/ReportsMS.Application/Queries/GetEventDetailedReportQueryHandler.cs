using MediatR;
using ReportsMS.Application.DTOs;
using ReportsMS.Domain.Interfaces;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ReportsMS.Application.Queries
{
    public class GetEventDetailedReportQueryHandler : IRequestHandler<GetEventDetailedReportQuery, EventDetailedReportDto?>
    {
        private readonly IReportsRepository _repository;

        public GetEventDetailedReportQueryHandler(IReportsRepository repository)
        {
            _repository = repository;
        }

        public async Task<EventDetailedReportDto?> Handle(GetEventDetailedReportQuery request, CancellationToken cancellationToken)
        {
            var sales = await _repository.GetSalesByEventAsync(request.EventId, cancellationToken);
            var stats = await _repository.GetEventStatsAsync(request.EventId, cancellationToken);

            if (stats == null && !sales.Any()) return null;

            var attendees = sales
                .Select(s => new AttendeeDto(s.UserEmail ?? "Desconocido", s.Date, s.Amount))
                .OrderByDescending(a => a.Date)
                .ToList();

            var dailySales = sales
                .GroupBy(s => s.Date.Date)
                .Select(g => new DailySaleDto(g.Key, g.Sum(x => x.Amount), g.Count()))
                .OrderBy(x => x.Date)
                .ToList();

            return new EventDetailedReportDto(
                request.EventId,
                stats?.TotalCapacity ?? 0,
                stats?.SoldSeats ?? sales.Count(),
                sales.Sum(s => s.Amount),
                attendees,
                dailySales
            );
        }
    }
}
