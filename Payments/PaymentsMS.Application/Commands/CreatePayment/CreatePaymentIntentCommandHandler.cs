using MediatR;
using PaymentsMS.Application.DTOs;
using PaymentsMS.Domain.Entities;
using PaymentsMS.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace PaymentsMS.Application.Commands.CreatePayment
{
    public class CreatePaymentIntentCommandHandler : IRequestHandler<CreatePaymentIntentCommand, PaymentIntentResponseDto>
    {
        private readonly IPaymentGateway _gateway;
        private readonly IPaymentRepository _repository;

        public CreatePaymentIntentCommandHandler(IPaymentGateway gateway, IPaymentRepository repository)
        {
            _gateway = gateway;
            _repository = repository;
        }

        public async Task<PaymentIntentResponseDto> Handle(CreatePaymentIntentCommand request, CancellationToken cancellationToken)
        {
            var data = request.PaymentData;

            var gatewayResponse = await _gateway.CreatePaymentIntentAsync(data.Amount, data.Currency, data.BookingId.ToString(), data.UserId);
            
            var payment = new Payment(data.BookingId, data.UserId, data.Amount, data.Currency, data.Email ?? string.Empty);
            payment.SetStripePaymentIntentId(gatewayResponse.Id);
            
            await _repository.AddAsync(payment, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            return new PaymentIntentResponseDto(gatewayResponse.ClientSecret, gatewayResponse.Id); 
        }
    }
}
