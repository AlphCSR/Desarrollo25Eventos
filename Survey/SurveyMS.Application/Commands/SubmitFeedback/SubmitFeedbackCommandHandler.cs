
using MediatR;
using SurveyMS.Domain.Entities;
using SurveyMS.Domain.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SurveyMS.Application.Commands.SubmitFeedback
{
    public class SubmitFeedbackCommandHandler : IRequestHandler<SubmitFeedbackCommand, Guid>
    {
        private readonly IFeedbackRepository _repository;

        public SubmitFeedbackCommandHandler(IFeedbackRepository repository)
        {
            _repository = repository;
        }

        public async Task<Guid> Handle(SubmitFeedbackCommand request, CancellationToken cancellationToken)
        {
            var alreadyRated = await _repository.HasUserRatedBookingAsync(request.UserId, request.BookingId, cancellationToken);
            if (alreadyRated)
            {
                throw new InvalidOperationException("Ya has enviado un comentario para esta reserva.");
            }

            var feedback = new Feedback(
                request.EventId,
                request.UserId,
                request.BookingId,
                request.UserName,
                request.Rating,
                request.Comment
            );

            await _repository.AddAsync(feedback, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            return feedback.Id;
        }
    }
}
