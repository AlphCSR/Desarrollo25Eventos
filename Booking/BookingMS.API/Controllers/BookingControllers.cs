using MediatR;
using Microsoft.AspNetCore.Mvc;
using BookingMS.Application.Commands.CreateBooking;
using BookingMS.Shared.Dtos.Response;
using System.Collections.Generic;
using System;
using System.Diagnostics.CodeAnalysis;

namespace BookingMS.Controllers
{
    public record CreateBookingRequest(Guid UserId, Guid EventId, List<Guid> SeatIds, decimal TotalAmount);

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
            // Aquí podrías validar que el Token JWT coincida con el UserId del request por seguridad
            
            var command = new CreateBookingCommand(request.UserId, request.EventId, request.SeatIds, request.TotalAmount);
            var result = await _mediator.Send(command);
            
            return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
        }

        [HttpGet("{id}")]
        public IActionResult Get(Guid id)
        {
            return Ok("Endpoint pendiente de implementar consulta (CQRS)");
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetBookingsByUser(Guid userId)
        {
            var query = new Application.Queries.GetBookingsByUser.GetBookingsByUserQuery(userId);
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
            var command = new Application.Commands.PayBooking.PayBookingCommand(id);
            var result = await _mediator.Send(command);
            if (!result) return NotFound("Reserva no encontrada");
            return Ok("Pago procesado exitosamente");
        }
    }
}