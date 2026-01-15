using MediatR;
using ReportsMS.Application.DTOs;
using ReportsMS.Domain.Interfaces;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ReportsMS.Application.Queries
{
    public class GetSalesReportQueryHandler : IRequestHandler<GetSalesReportQuery, SalesReportDto>
    {
        private readonly IReportsRepository _repository;

        public GetSalesReportQueryHandler(IReportsRepository repository)
        {
            _repository = repository;
        }

        public async Task<SalesReportDto> Handle(GetSalesReportQuery request, CancellationToken cancellationToken)
        {
            var sales = await _repository.GetSalesByEventAsync(request.EventId, cancellationToken);
            
            var totalCount = sales.Count();
            var totalRevenue = sales.Sum(s => s.Amount);

            return new SalesReportDto(request.EventId, totalCount, totalRevenue);
        }
    }
}
