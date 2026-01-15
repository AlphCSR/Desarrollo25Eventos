using MediatR;
using Microsoft.AspNetCore.Mvc;
using ServicesMS.Application.Commands;
using ServicesMS.Application.DTOs;
using ServicesMS.Application.Queries;
using System;
using System.Threading.Tasks;

namespace ServicesMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServicesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ServicesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("event/{eventId}")]
        public async Task<IActionResult> GetByEvent(Guid eventId)
        {
            var result = await _mediator.Send(new GetServicesByEventQuery(eventId));
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateServiceDto dto)
        {
            var id = await _mediator.Send(new CreateServiceCommand(dto));
            return CreatedAtAction(nameof(GetByEvent), new { eventId = dto.EventId }, new { id });
        }

        [HttpPost("book")]
        public async Task<IActionResult> Book([FromBody] BookServiceDto dto)
        {
            try 
            {
                await _mediator.Send(new BookServiceCommand(dto));
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetServiceByIdQuery(id));
            if (result == null) return NotFound();
            return Ok(result);
        }
    }
}
