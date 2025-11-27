using MediatR;
using Microsoft.AspNetCore.Mvc;
using UsersMS.Application.Commands.CreateUser;
using UsersMS.Application.Commands.DeleteUser;
using UsersMS.Application.Commands.UpdateUser;
using UsersMS.Application.DTOs;
using UsersMS.Application.Queries.GetAllUsers;
using UsersMS.Application.Queries.GetUserById;
using UsersMS.Application.Queries.GetUserByEmail;
using UsersMS.Domain.Exceptions;
using UsersMS.Application.Commands;

namespace UsersMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
        {
            try
            {
                var command = new CreateUserCommand(dto);
                var userId = await _mediator.Send(command);
                return CreatedAtAction(nameof(Create), new { id = userId }, userId);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex) 
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetUserByIdQuery(id));
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("email/{email}")]
        public async Task<IActionResult> GetByEmail(string email)
        {
            var result = await _mediator.Send(new GetUserByEmailQuery(email));
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetAllUsersQuery());
            return Ok(result);
        }

        [HttpPut("id/{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.FullName)) return BadRequest("El nombre es requerido.");
            try
            {
                var command = new UpdateUserCommand(id, dto);
                await _mediator.Send(command);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var command = new DeleteUserCommand(id);
                await _mediator.Send(command);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPut("{id}/preferences")]
        public async Task<IActionResult> UpdatePreferences(Guid id, [FromBody] UpdateUserPreferencesDto dto)
        {
            try
            {
                var command = new UpdateUserPreferencesCommand(id, dto);
                await _mediator.Send(command);
                return NoContent();
            }
            catch (UserNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}