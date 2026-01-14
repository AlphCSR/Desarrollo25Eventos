using MediatR;
using System;

namespace CommunityMS.Application.Commands
{
    public class AddPostCommand : IRequest<Guid>
    {
        public Guid ThreadId { get; set; }
        public string Content { get; set; }
        public Guid AuthorId { get; set; }
        public string AuthorName { get; set; }
    }
}
