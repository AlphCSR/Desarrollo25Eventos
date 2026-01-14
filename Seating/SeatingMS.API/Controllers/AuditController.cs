using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Threading.Tasks;
using SeatingMS.Application.Queries;

namespace SeatingMS.API.Controllers
{
    [ApiController]
    [Route("api/seating/audit")]
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
    }
}
