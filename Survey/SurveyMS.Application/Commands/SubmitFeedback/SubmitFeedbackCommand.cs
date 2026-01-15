
using MediatR;
using System;

namespace SurveyMS.Application.Commands.SubmitFeedback
{
    public record SubmitFeedbackCommand(
        Guid EventId, 
        Guid UserId, 
        Guid BookingId, 
        string UserName,
        int Rating, 
        string Comment
    ) : IRequest<Guid>;
}
