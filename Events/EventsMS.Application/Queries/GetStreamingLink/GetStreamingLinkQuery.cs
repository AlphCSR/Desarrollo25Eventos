using MediatR;
using System;

namespace EventsMS.Application.Queries.GetStreamingLink
{
    public record GetStreamingLinkQuery(Guid EventId) : IRequest<string>;
}
