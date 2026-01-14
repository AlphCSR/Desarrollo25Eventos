using MediatR;
using System;

namespace CommunityMS.Application.Commands
{
    public class CreateThreadCommand : IRequest<Guid>
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public Guid EventId { get; set; }
        public Guid AuthorId { get; set; }
        public string AuthorName { get; set; }
    }
}
