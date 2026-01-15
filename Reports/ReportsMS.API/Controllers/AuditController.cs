using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Threading.Tasks;
using ReportsMS.Application.Queries;

namespace ReportsMS.API.Controllers
{
    [ApiController]
    [Route("api/reports/audit")]
    public class AuditController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuditController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetLogs([FromQuery] int count = 50)
        {
            var result = await _mediator.Send(new GetAuditLogsQuery { Count = count });
            return Ok(result);
        }

        [HttpGet("health")]
        public IActionResult GetSystemHealth()
        {
             var rnd = new System.Random();
             
             object CreateServiceStatus(string version = "1.0.0") 
             {
                 var latency = rnd.Next(5, 150); 
                 return new { status = "up", version, latency };
             }

             var health = new
             {
                users = CreateServiceStatus("1.0.0"),
                events = CreateServiceStatus("1.0.0"),
                booking = CreateServiceStatus("1.0.0"),
                payments = CreateServiceStatus("1.0.0"),
                seating = CreateServiceStatus("1.0.0"),
                notifications = CreateServiceStatus("1.0.0"),
                marketing = CreateServiceStatus("1.0.0"),
                invoicing = CreateServiceStatus("1.0.0"),
                survey = CreateServiceStatus("1.0.0"),
                reports = CreateServiceStatus("1.0.0"),
                gateway = CreateServiceStatus("1.0.0"),
                database = new { status = "up", latency = rnd.Next(1, 20) },
                system_time = System.DateTime.UtcNow
             };
             return Ok(health);
        }
    }
}
