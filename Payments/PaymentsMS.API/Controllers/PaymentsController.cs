using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaymentsMS.Application.Commands.CreatePayment;
using PaymentsMS.Application.DTOs;
using System.Threading.Tasks;

namespace PaymentsMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PaymentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] CreatePaymentDto dto)
        {
            var result = await _mediator.Send(new CreatePaymentIntentCommand(dto));
            return Ok(result);
        }
    }
}
