using MediatR;
using Microsoft.AspNetCore.Mvc;
using ReportsMS.Application.Queries;
using System;
using System.Threading.Tasks;

namespace ReportsMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ReportsMS.Infrastructure.Services.CsvExportService _csvService;

        public ReportsController(IMediator mediator, ReportsMS.Infrastructure.Services.CsvExportService csvService)
        {
            _mediator = mediator;
            _csvService = csvService;
        }

        [HttpGet("sales/{eventId}")]
        public async Task<IActionResult> GetSalesByEvent(Guid eventId)
        {
            var result = await _mediator.Send(new GetSalesReportQuery(eventId));
            return Ok(result);
        }

        [HttpGet("detailed/{eventId}")]
        public async Task<IActionResult> GetDetailedReport(Guid eventId)
        {
            var result = await _mediator.Send(new GetEventDetailedReportQuery(eventId));
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("live")]
        public async Task<IActionResult> GetLiveReport([FromQuery] string type)
        {
            var result = await _mediator.Send(new GetLiveReportQuery { ReportType = type ?? "summary" });
            return Ok(result);
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardMetrics()
        {
            var result = await _mediator.Send(new GetDashboardMetricsQuery());
            return Ok(result);
        }

        [HttpPost("generate")]
        public IActionResult GenerateReport([FromBody] object reportRequest)
        {
             return Ok(new { message = "Se ha solicitado el reporte. Recibirás una notificación cuando esté listo.", downloadUrl = "https://example.com/report-sample.pdf" });
        }
        [HttpGet("export/{eventId}/csv")]
        public async Task<IActionResult> ExportSalesCsv(Guid eventId)
        {
            var content = await _csvService.GenerateSalesCsvAsync(eventId);
            return File(content, "text/csv", $"sales_report_{eventId}.csv");
        }
    }
}
