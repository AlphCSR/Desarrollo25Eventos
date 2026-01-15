
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SurveyMS.Application.Commands.SubmitFeedback;
using SurveyMS.Application.Queries.GetEventFeedback;
using System;
using System.Threading.Tasks;

namespace SurveyMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeedbacksController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FeedbacksController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Submit([FromBody] SubmitFeedbackCommand command)
        {
            try
            {
                var id = await _mediator.Send(command);
                return Ok(new { Id = id });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("event/{eventId}")]
        public async Task<IActionResult> GetByEvent(Guid eventId)
        {
            var result = await _mediator.Send(new GetEventFeedbackQuery(eventId));
            return Ok(result);
        }
    }
}
