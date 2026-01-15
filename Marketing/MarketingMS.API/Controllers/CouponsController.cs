using MediatR;
using Microsoft.AspNetCore.Mvc;
using MarketingMS.Application.Commands.CreateCoupon;
using MarketingMS.Application.Queries.ValidateCoupon;
using System;
using System.Threading.Tasks;

namespace MarketingMS.API.Controllers
{
    [ApiController]
    [Route("api/coupons")]
    public class CouponsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CouponsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new MarketingMS.Application.Queries.GetAllCoupons.GetAllCouponsQuery());
            return Ok(result);
        }

        [HttpGet("validate")]
        public async Task<IActionResult> Validate([FromQuery] string code, [FromQuery] decimal amount)
        {
            var result = await _mediator.Send(new ValidateCouponQuery(code, amount));
            if (result == null) return NotFound("Cupón no encontrado o inválido");
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCouponCommand command)
        {
            try 
            {
                var id = await _mediator.Send(command);
                return Ok(new { id });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _mediator.Send(new MarketingMS.Application.Commands.DeleteCoupon.DeleteCouponCommand(id));
            return NoContent();
        }
    }
}
