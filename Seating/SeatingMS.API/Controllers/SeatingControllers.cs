using MediatR;
using Microsoft.AspNetCore.Mvc;
using SeatingMS.Application.Commands.LockSeat;
using SeatingMS.Application.Queries.GetSeatsByEvent;
using SeatingMS.Application.DTOs;
using SeatingMS.Shared.Dtos;
using SeatingMS.Application.Commands.UnlockSeat;

using System.Diagnostics.CodeAnalysis;

namespace SeatingMS.API.Controllers
{
    [ExcludeFromCodeCoverage]
    [ApiController]
    [Route("api/[controller]")]
    public class SeatingController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SeatingController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("lock")]
        public async Task<IActionResult> LockSeat([FromBody] LockSeatRequestDto request)
        {
            try 
            {
                var result = await _mediator.Send(new LockSeatCommand(request.SeatId, request.UserId));
                if (result) return Ok("Asiento bloqueado exitosamente.");
                return BadRequest("El asiento no está disponible.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("unlock")]
        public async Task<IActionResult> UnlockSeat([FromBody] UnlockSeatRequestDto request)
        {
            try 
            {
                var result = await _mediator.Send(new UnlockSeatCommand(request.SeatId, request.UserId));
                if (result) return Ok("Asiento desbloqueado exitosamente.");
                return BadRequest("El asiento no está bloqueado.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("event/{eventId}")]
        public async Task<IActionResult> GetSeatsByEvent(Guid eventId)
        {
            var result = await _mediator.Send(new GetSeatsByEventQuery(eventId));
            return Ok(result);
        }
    }
}