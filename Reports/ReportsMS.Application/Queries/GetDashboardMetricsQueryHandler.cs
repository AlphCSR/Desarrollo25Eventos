using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using ReportsMS.Domain.Entities;
using ReportsMS.Domain.Interfaces;

namespace ReportsMS.Application.Queries
{
    public class GetDashboardMetricsQueryHandler : IRequestHandler<GetDashboardMetricsQuery, IEnumerable<DashboardMetric>>
    {
        private readonly IReportsRepository _repository;

        public GetDashboardMetricsQueryHandler(IReportsRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<DashboardMetric>> Handle(GetDashboardMetricsQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetAllMetricsAsync(cancellationToken);
        }
    }
}
