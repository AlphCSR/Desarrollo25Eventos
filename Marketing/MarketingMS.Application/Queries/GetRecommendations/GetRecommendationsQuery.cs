using MediatR;
using MarketingMS.Application.Interfaces;

namespace MarketingMS.Application.Queries.GetRecommendations
{
    public record GetRecommendationsQuery(Guid UserId) : IRequest<IEnumerable<EventDetailsDto>>;
}
