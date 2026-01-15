using MediatR;
using System;

namespace ReportsMS.Application.Queries
{
    public class GetLiveReportQuery : IRequest<object>
    {
        public string ReportType { get; set; } = null!;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
