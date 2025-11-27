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
    /// <summary>
    /// Controlador para la gestión de usuarios.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Crea un nuevo usuario.
        /// </summary>
        /// <param name="dto">Datos del usuario a crear.</param>
        /// <returns>El ID del usuario creado.</returns>
        /// <response code="201">Usuario creado exitosamente.</response>
        /// <response code="400">Si los datos son inválidos o el usuario ya existe.</response>
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

        /// <summary>
        /// Obtiene un usuario por su ID.
        /// </summary>
        /// <param name="id">ID del usuario.</param>
        /// <returns>Datos del usuario.</returns>
        /// <response code="200">Usuario encontrado.</response>
        /// <response code="404">Usuario no encontrado.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetUserByIdQuery(id));
            if (result == null) return NotFound();
            return Ok(result);
        }

        /// <summary>
        /// Obtiene un usuario por su email.
        /// </summary>
        /// <param name="email">Email del usuario.</param>
        /// <returns>Datos del usuario.</returns>
        /// <response code="200">Usuario encontrado.</response>
        /// <response code="404">Usuario no encontrado.</response>
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

        /// <summary>
        /// Obtiene todos los usuarios.
        /// </summary>
        /// <returns>Lista de usuarios.</returns>
        /// <response code="200">Lista de usuarios obtenida exitosamente.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetAllUsersQuery());
            return Ok(result);
        }

        /// <summary>
        /// Actualiza un usuario existente.
        /// </summary>
        /// <param name="id">ID del usuario a actualizar.</param>
        /// <param name="dto">Nuevos datos del usuario.</param>
        /// <returns>No Content si la actualización fue exitosa.</returns>
        /// <response code="204">Usuario actualizado exitosamente.</response>
        /// <response code="400">Si los datos son inválidos.</response>
        /// <response code="404">Usuario no encontrado.</response>
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

        /// <summary>
        /// Elimina (desactiva) un usuario.
        /// </summary>
        /// <param name="id">ID del usuario a eliminar.</param>
        /// <returns>No Content si la eliminación fue exitosa.</returns>
        /// <response code="204">Usuario eliminado exitosamente.</response>
        /// <response code="404">Usuario no encontrado.</response>
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

        /// <summary>
        /// Actualiza las preferencias de un usuario.
        /// </summary>
        /// <param name="id">ID del usuario.</param>
        /// <param name="dto">Nuevas preferencias.</param>
        /// <returns>No Content si la actualización fue exitosa.</returns>
        /// <response code="204">Preferencias actualizadas exitosamente.</response>
        /// <response code="400">Si los datos son inválidos.</response>
        /// <response code="404">Usuario no encontrado.</response>
        [HttpPut("{id}/preferences")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
            catch (InvalidUserDataException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}