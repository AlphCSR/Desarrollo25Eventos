using CommunityMS.Application.Commands;
using CommunityMS.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CommunityMS.API.Controllers
{
    [ApiController]
    [Route("api/community")]
    public class ForumController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ForumController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("threads")]
        public async Task<IActionResult> CreateThread([FromBody] CreateThreadCommand command, CancellationToken cancellationToken)
        {
            var threadId = await _mediator.Send(command, cancellationToken);
            return Ok(new { id = threadId });
        }

        [HttpGet("events/{eventId}/threads")]
        public async Task<IActionResult> GetThreadsByEvent(Guid eventId, CancellationToken cancellationToken)
        {
            var threads = await _mediator.Send(new GetThreadsByEventIdQuery(eventId), cancellationToken);
            return Ok(threads);
        }

        [HttpGet("threads/{threadId}")]
        public async Task<IActionResult> GetThread(Guid threadId, CancellationToken cancellationToken)
        {
            var thread = await _mediator.Send(new GetThreadByIdQuery(threadId), cancellationToken);
            return Ok(thread);
        }

        [HttpPost("posts")]
        public async Task<IActionResult> AddPost([FromBody] AddPostCommand command, CancellationToken cancellationToken)
        {
            var postId = await _mediator.Send(command, cancellationToken);
            return Ok(new { id = postId });
        }
    }
}
