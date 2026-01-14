using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using UsersMS.Application.Commands.CreateUser;
using UsersMS.Application.Commands.DeleteUser;
using UsersMS.Application.Commands.UpdateUser;
using UsersMS.Application.DTOs;
using UsersMS.Application.Queries.GetAllUsers;
using UsersMS.Application.Queries.GetUserById;
using UsersMS.Application.Queries.GetUserByEmail;
using UsersMS.Domain.Exceptions;
using UsersMS.Application.Queries.GetUserHistory;
using UsersMS.Shared.Enums;

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
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
        {
            try
            {
                var command = new CreateUserCommand(dto);
                var userId = await _mediator.Send(command);
                return CreatedAtAction(nameof(Create), new { id = userId }, userId);
            }
            catch (UserAlreadyExistsException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidUserDataException ex) 
            {
                return BadRequest(ex.Message);
            }
            catch (KeycloakIntegrationException ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetUserByIdQuery(id));
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("email/{email}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByEmail(string email)
        {
            try
            {
                var result = await _mediator.Send(new GetUserByEmailQuery(email));
                return Ok(result);
            }
            catch (UserNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetAllUsersQuery());
            return Ok(result);
        }

        [HttpPut("id/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.FullName)) return BadRequest("El nombre es requerido.");
            try
            {
                var command = new UpdateUserCommand(id, dto);
                await _mediator.Send(command);
                return NoContent();
            }
            catch (UserNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidUserDataException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var command = new DeleteUserCommand(id);
                await _mediator.Send(command);
                return NoContent();
            }
            catch (UserNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("{id}/history")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetHistory(Guid id)
        {
             var result = await _mediator.Send(new GetUserHistoryQuery(id));
             return Ok(result);
        }
 
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMe()
        {
            var subClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(subClaim))
            {
                return Unauthorized("Token inválido: falta 'sub' claim.");
            }

            var emailClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(emailClaim)) return Unauthorized("Token inválido: falta 'email' claim.");

            try
            {
                var user = await _mediator.Send(new GetUserByEmailQuery(emailClaim));
                return Ok(user);
            }
            catch (UserNotFoundException)
            {
                var nameClaim = User.Claims.FirstOrDefault(c => c.Type == "name")?.Value ?? 
                                User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "Unknown";
                
                var role = UserRole.User;
                if (User.IsInRole("admin")) role = UserRole.Admin;
                else if (User.IsInRole("organizer")) role = UserRole.Organizer;

                var dto = new CreateUserDto(nameClaim, emailClaim, "jit-password-ignored", role, subClaim);
                
                var command = new CreateUserCommand(dto);
                var userId = await _mediator.Send(command);
                
                var newUser = await _mediator.Send(new GetUserByIdQuery(userId));
                return Ok(newUser);
            }
        }
    }
}