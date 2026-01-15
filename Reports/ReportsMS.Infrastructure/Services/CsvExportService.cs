using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ReportsMS.Domain.Interfaces;

namespace ReportsMS.Infrastructure.Services
{
    public class CsvExportService
    {
        private readonly IReportsRepository _repository;

        public CsvExportService(IReportsRepository repository)
        {
            _repository = repository;
        }

        public async Task<byte[]> GenerateSalesCsvAsync(Guid eventId)
        {
            var sales = await _repository.GetSalesByEventAsync(eventId, default);
            var sb = new StringBuilder();

            sb.AppendLine("BookingID,Email,Date,Amount,Status");

            foreach (var sale in sales)
            {
                sb.AppendLine($"{sale.BookingId},{sale.UserEmail},{sale.Date:yyyy-MM-dd HH:mm:ss},{sale.Amount},Confirmed");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }
    }
}
