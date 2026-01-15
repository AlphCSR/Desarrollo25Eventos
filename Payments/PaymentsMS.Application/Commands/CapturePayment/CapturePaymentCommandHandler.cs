using MediatR;
using PaymentsMS.Domain.Interfaces;
using BookingMS.Shared.Events;
using System.Threading;
using System.Threading.Tasks;
using System;
using MassTransit;

namespace PaymentsMS.Application.Commands.CapturePayment
{
    public class CapturePaymentCommandHandler : IRequestHandler<CapturePaymentCommand, bool>
    {
        private readonly IPaymentRepository _repository;
        private readonly IPublishEndpoint _publishEndpoint;

        public CapturePaymentCommandHandler(IPaymentRepository repository, IPublishEndpoint publishEndpoint)
        {
            _repository = repository;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<bool> Handle(CapturePaymentCommand request, CancellationToken cancellationToken)
        {
            var payment = await _repository.GetByPaymentIntentIdAsync(request.PaymentIntentId, cancellationToken);
            if (payment == null) return false;

            if (payment.Status == "Succeeded") return true;

            payment.MarkAsSucceeded();
            await _repository.UpdateAsync(payment, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            await _publishEndpoint.Publish(new PaymentCapturedEvent
            {
                BookingId = payment.BookingId,
                UserId = payment.UserId,
                Email = payment.Email,
                TransactionId = payment.StripePaymentIntentId,
                PaymentDate = DateTime.UtcNow,
                Amount = payment.Amount
            }, cancellationToken);

            return true;
        }
    }
}
