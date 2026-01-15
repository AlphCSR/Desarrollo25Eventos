using MediatR;

namespace PaymentsMS.Application.Commands.FailPayment
{
    public record FailPaymentCommand(string PaymentIntentId, string Reason) : IRequest<bool>;
}
