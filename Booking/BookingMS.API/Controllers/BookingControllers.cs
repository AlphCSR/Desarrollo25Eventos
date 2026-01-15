using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BookingMS.Application.Commands.CreateBooking;
using BookingMS.Shared.Dtos.Response;
using System.Collections.Generic;
using System;
using System.Diagnostics.CodeAnalysis;

namespace BookingMS.Controllers
{
    public record CreateBookingRequest(Guid UserId, Guid EventId, List<Guid> SeatIds, List<Guid>? ServiceIds, decimal TotalAmount, string? UserEmail = null, string? CouponCode = null);

    [ExcludeFromCodeCoverage]
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BookingController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBookingRequest request)
        {
            var command = new CreateBookingCommand(
                request.UserId, 
                request.EventId, 
                request.SeatIds, 
                request.ServiceIds ?? new List<Guid>(), 
                request.TotalAmount, 
                request.UserEmail ?? "unknown@example.com",
                request.CouponCode);
            var result = await _mediator.Send(command);
            
            return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var query = new Application.Queries.GetBookingById.GetBookingByIdQuery(id);
            var result = await _mediator.Send(query);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("user/{userId}")]

        public async Task<IActionResult> GetBookingsByUser(Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var query = new Application.Queries.GetBookingsByUser.GetBookingsByUserQuery(userId, page, pageSize);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveBooking([FromQuery] Guid userId, [FromQuery] Guid eventId)
        {
            var query = new Application.Queries.GetActiveBooking.GetActiveBookingQuery(userId, eventId);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost("pay/{id}")]
        public async Task<IActionResult> Pay(Guid id)
        {
            var language = Request.Headers["Accept-Language"].ToString().Split(',')[0].Trim().ToLower();
            if (string.IsNullOrEmpty(language) || !language.StartsWith("en")) language = "es";
            else language = "en";

            var command = new Application.Commands.PayBooking.PayBookingCommand(id, language);
            var result = await _mediator.Send(command);
            if (!result) return NotFound("Reserva no encontrada");
            return Ok("Pago procesado exitosamente");
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Cancel(Guid id, [FromQuery] string reason = "User Cancelled")
        {
            var language = Request.Headers["Accept-Language"].ToString().Split(',')[0].Trim().ToLower();
            if (string.IsNullOrEmpty(language) || !language.StartsWith("en")) language = "es";
            else language = "en";

            var command = new Application.Commands.CancelBooking.CancelBookingCommand(id, reason, language);
            var result = await _mediator.Send(command);
            if (!result) return NotFound("Reserva no encontrada");
            return Ok("Reserva cancelada exitosamente");
        }
    }
}