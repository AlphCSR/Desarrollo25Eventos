using MediatR;
using System;
using CommunityMS.Domain.Entities;

namespace CommunityMS.Application.Queries
{
    public class GetThreadByIdQuery : IRequest<ForumThread>
    {
        public Guid ThreadId { get; set; }

        public GetThreadByIdQuery(Guid threadId)
        {
            ThreadId = threadId;
        }
    }
}
