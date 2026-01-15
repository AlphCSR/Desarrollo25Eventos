using MediatR;
using PaymentsMS.Application.DTOs;

namespace PaymentsMS.Application.Commands.CreatePayment
{
    public record CreatePaymentIntentCommand(CreatePaymentDto PaymentData) : IRequest<PaymentIntentResponseDto>;
}
