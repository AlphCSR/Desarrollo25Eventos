using MediatR;
using System.Collections.Generic;
using ReportsMS.Domain.Entities;

namespace ReportsMS.Application.Queries
{
    public class GetDashboardMetricsQuery : IRequest<IEnumerable<DashboardMetric>>
    {
    }
}
