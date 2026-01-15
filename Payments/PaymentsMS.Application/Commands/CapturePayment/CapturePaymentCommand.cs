using MediatR;
using System.Threading.Tasks;

namespace PaymentsMS.Application.Commands.CapturePayment
{
    public record CapturePaymentCommand(string PaymentIntentId) : IRequest<bool>;
}
