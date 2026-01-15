using MediatR;
using ReportsMS.Domain.Interfaces;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ReportsMS.Application.Queries
{
    public class GetLiveReportQueryHandler : IRequestHandler<GetLiveReportQuery, object>
    {
        private readonly IReportsRepository _repository;

        public GetLiveReportQueryHandler(IReportsRepository repository)
        {
            _repository = repository;
        }

        public async Task<object> Handle(GetLiveReportQuery request, CancellationToken cancellationToken)
        {

            
            switch (request.ReportType.ToLower())
            {
                case "sales":
                    var sales = await _repository.GetDailySalesAsync(cancellationToken);
                    return new { title = "Ventas Diarias (Últimos 30 días)", data = sales, columns = new[] { "Date", "Total", "Count" } };

                case "attendance":
                    var attendance = await _repository.GetEventOccupancyAsync(cancellationToken);
                    return new { title = "Ocupación de Eventos", data = attendance, columns = new[] { "EventId", "Capacity", "Sold", "Occupancy" } };

                case "summary":
                default:
                    var metrics = await _repository.GetAllMetricsAsync(cancellationToken);
                    return new 
                    { 
                        title = "Resumen Ejecutivo (Métricas Cacheadas)", 
                        data = metrics.Select(m => new { Metric = m.MetricName, Value = m.Value.ToString() }),
                        columns = new[] { "Metric", "Value" }
                    };
            }
        }
    }
}
