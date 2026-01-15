using MediatR;
using PaymentsMS.Domain.Interfaces;
using BookingMS.Shared.Events;
using System.Threading;
using MassTransit;
using System.Threading.Tasks;

namespace PaymentsMS.Application.Commands.FailPayment
{
    public class FailPaymentCommandHandler : IRequestHandler<FailPaymentCommand, bool>
    {
        private readonly IPaymentRepository _repository;
        private readonly IPublishEndpoint _publishEndpoint;

        public FailPaymentCommandHandler(IPaymentRepository repository, IPublishEndpoint publishEndpoint)
        {
            _repository = repository;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<bool> Handle(FailPaymentCommand request, CancellationToken cancellationToken)
        {
            var payment = await _repository.GetByPaymentIntentIdAsync(request.PaymentIntentId, cancellationToken);
            if (payment == null) return false;

            if (payment.Status == "Failed") return true;

            payment.MarkAsFailed();
            await _repository.UpdateAsync(payment, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            await _publishEndpoint.Publish(new PaymentFailedEvent
            {
                BookingId = payment.BookingId,
                UserId = payment.UserId,
                Email = payment.Email,
                Reason = request.Reason
            }, cancellationToken);

            return true;
        }
    }
}
