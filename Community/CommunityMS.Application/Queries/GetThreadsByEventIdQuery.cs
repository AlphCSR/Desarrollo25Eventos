using MediatR;
using System;
using System.Collections.Generic;
using CommunityMS.Domain.Entities;

namespace CommunityMS.Application.Queries
{
    public class GetThreadsByEventIdQuery : IRequest<IEnumerable<ForumThread>>
    {
        public Guid EventId { get; set; }

        public GetThreadsByEventIdQuery(Guid eventId)
        {
            EventId = eventId;
        }
    }
}
