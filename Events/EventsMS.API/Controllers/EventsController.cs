using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EventsMS.Application.Commands.CancelEvent;
using EventsMS.Application.Commands.CreateEvent;
using EventsMS.Application.Commands.PublishEvent;
using EventsMS.Application.Commands.UpdateEvent;
using EventsMS.Application.Commands.UpdateEventStatus;
using EventsMS.Application.Commands.UploadImage;
using EventsMS.Application.Queries.GetStreamingLink;
using EventsMS.Domain.Exceptions;
using EventsMS.Application.DTOs;
using EventsMS.Application.Queries.GetEvents;
using EventsMS.Application.Queries.GetEventById;
using EventsMS.Application.Queries.GetEventSections;
using System.Security.Claims;
using EventsMS.Shared.Enums;
using EventsMS.Application.Commands.UploadImage;
using EventsMS.Application.Commands.PublishEvent;
using EventsMS.Application.Commands.UpdateEventStatus;
using EventsMS.Application.Commands.CancelEvent;

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

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEventDto dto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                         ?? User.FindFirst("sub")?.Value;
            
            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized("User ID not found in token.");

            var isAdmin = User.IsInRole("Admin") || User.IsInRole("admin") || User.FindFirst("email")?.Value == "admin@admin.com";

            try
            {
                var existingEvent = await _mediator.Send(new GetEventByIdQuery(id));
                if (existingEvent == null) return NotFound();

                if (!isAdmin && existingEvent.IdUser != userId)
                    return Forbid("No tienes permiso para modificar este evento.");

                var command = new UpdateEventCommand(id, dto.Title, dto.Description, dto.Date, dto.EndDate, dto.VenueName, dto.Categories, dto.Type, dto.StreamingUrl);
                await _mediator.Send(command);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 100,
            [FromQuery] string? category = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] int? type = null,
            [FromQuery] bool futureOnly = false)
        {
            var email = User.FindFirst("email")?.Value 
                     ?? User.FindFirst("preferred_username")?.Value 
                     ?? User.FindFirst(ClaimTypes.Email)?.Value
                     ?? User.FindFirst(ClaimTypes.Name)?.Value;

            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            var isPrivileged = roles.Any(r => r.Equals("admin", StringComparison.OrdinalIgnoreCase) || r.Equals("organizer", StringComparison.OrdinalIgnoreCase)) 
                           || User.IsInRole("Admin") 
                           || User.IsInRole("admin")
                           || User.IsInRole("Organizer")
                           || User.IsInRole("organizer")
                           || email == "admin@admin.com";

            var query = new GetEventsQuery 
            { 
                IncludeAll = isPrivileged, 
                Category = category, 
                FutureEventsOnly = futureOnly,
                Page = page,
                PageSize = pageSize,
                StartDate = startDate,
                EndDate = endDate,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                Type = type
            };

            var result = await _mediator.Send(query);
            return Ok(result);
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

        [HttpPost("{id}/image")]
        public async Task<IActionResult> UploadImage(Guid id, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            using var stream = file.OpenReadStream();
            var command = new UploadEventImageCommand(id, stream, file.FileName);
            var url = await _mediator.Send(command);

            return Ok(new { Url = url });
        }
        [HttpPost("{id}/publish")]
        public async Task<IActionResult> Publish(Guid id)
        {
            var existingEvent = await _mediator.Send(new GetEventByIdQuery(id));
            if (existingEvent == null) return NotFound();

            var command = new PublishEventCommand(id);
            var result = await _mediator.Send(command);
            if (!result) return BadRequest("No se pudo publicar el evento. Asegúrate de que tenga secciones/localidades configuradas.");
            return Ok("Evento publicado exitosamente");
        }

        [Authorize(Roles = "Admin,admin")]
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromQuery] EventStatus status)
        {
            var command = new UpdateEventStatusCommand(id, status);
            var result = await _mediator.Send(command);
            if (!result) return NotFound();
            return Ok("Estado actualizado exitosamente");
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Cancel(Guid id, [FromQuery] string reason = "Organizer Cancelled")
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                         ?? User.FindFirst("sub")?.Value;

            Guid userId;
            if (!Guid.TryParse(userIdStr, out userId))
            {
                userId = Guid.Empty;
            }
            var isAdmin = true;

            try
            {
                var command = new CancelEventCommand(id, userId, isAdmin, reason);
                var result = await _mediator.Send(command);
                if (!result) return NotFound();
                return Ok("Evento cancelado exitosamente");
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("{id}/stream")]
        public async Task<IActionResult> GetStreamingLink(Guid id)
        {
            var streamUrl = await _mediator.Send(new GetStreamingLinkQuery(id));
            if (string.IsNullOrEmpty(streamUrl)) return NotFound("Streaming link not available.");

            return Ok(new { url = streamUrl });
        }
    }
}