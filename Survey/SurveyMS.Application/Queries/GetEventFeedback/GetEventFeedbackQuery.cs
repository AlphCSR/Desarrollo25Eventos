
using MediatR;
using SurveyMS.Domain.Entities;
using SurveyMS.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SurveyMS.Application.Queries.GetEventFeedback
{
    public record GetEventFeedbackQuery(Guid EventId) : IRequest<IEnumerable<FeedbackDto>>;

    public record FeedbackDto(Guid Id, Guid UserId, string UserName, int Rating, string Comment, DateTime CreatedAt);

    public class GetEventFeedbackQueryHandler : IRequestHandler<GetEventFeedbackQuery, IEnumerable<FeedbackDto>>
    {
        private readonly IFeedbackRepository _repository;

        public GetEventFeedbackQueryHandler(IFeedbackRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<FeedbackDto>> Handle(GetEventFeedbackQuery request, CancellationToken cancellationToken)
        {
            var feedbacks = await _repository.GetByEventIdAsync(request.EventId, cancellationToken);
            return feedbacks.Select(f => new FeedbackDto(f.Id, f.UserId, f.UserName, f.Rating, f.Comment, f.CreatedAt));
        }
    }
}
