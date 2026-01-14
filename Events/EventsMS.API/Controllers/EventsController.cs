using MediatR;
using Microsoft.AspNetCore.Mvc;
using EventsMS.Application.Commands.CreateEvent;
using EventsMS.Application.DTOs;
using EventsMS.Application.Queries.GetEvents;
using EventsMS.Application.Queries.GetEventById;
using EventsMS.Application.Queries.GetEventSections;

namespace EventsMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public EventsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEventDto dto)
        {
            try 
            {
                var id = await _mediator.Send(new CreateEventCommand(dto));
                return CreatedAtAction(nameof(Create), new { id }, id);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var events = await _mediator.Send(new GetEventsQuery());
            return Ok(events);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var eventDto = await _mediator.Send(new GetEventByIdQuery(id));
            if (eventDto == null) return NotFound();
            return Ok(eventDto);
        }

        [HttpGet("{id}/sections")]
        public async Task<IActionResult> GetSections(Guid id)
        {
            var sections = await _mediator.Send(new GetEventSectionsQuery(id));
            return Ok(sections);
        }
    }
}