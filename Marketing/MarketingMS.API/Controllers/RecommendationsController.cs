using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingMS.Application.Queries.GetRecommendations;
using System.Security.Claims;

namespace MarketingMS.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RecommendationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RecommendationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetRecommendations()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                         ?? User.FindFirst("sub")?.Value;

            if (userIdStr == null)
            {
                return Unauthorized("Usuario no autenticado para recomendaciones.");
            }

            if (!Guid.TryParse(userIdStr, out var userId))
                return BadRequest("ID de usuario inválido.");

            Console.WriteLine($"[MarketingMS] Obteniendo recomendaciones para User: {userId}");

            var result = await _mediator.Send(new GetRecommendationsQuery(userId));
            return Ok(result);
        }
    }
}
