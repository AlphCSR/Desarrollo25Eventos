using MediatR;
using Microsoft.Extensions.Logging;
using PaymentsMS.Application.Commands.CapturePayment;
using PaymentsMS.Application.Commands.FailPayment;
using PaymentsMS.Domain.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace PaymentsMS.Infrastructure.BackgroundJobs
{
    [ExcludeFromCodeCoverage]
    public class PaymentJobs
    {
        private readonly IPaymentRepository _repository;
        private readonly IPaymentGateway _gateway;
        private readonly IMediator _mediator;
        private readonly ILogger<PaymentJobs> _logger;

        public PaymentJobs(IPaymentRepository repository, IPaymentGateway gateway, IMediator mediator, ILogger<PaymentJobs> logger)
        {
            _repository = repository;
            _gateway = gateway;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task ReconcilePaymentsAsync()
        {
            _logger.LogInformation("Hangfire: Iniciando reconciliación de pagos...");
            
            var cutoffDate = DateTime.UtcNow.AddMinutes(-2);
            var pendingPayments = await _repository.GetPendingPaymentsOlderThanAsync(cutoffDate, CancellationToken.None);

            if (!pendingPayments.Any())
            {
                _logger.LogInformation("Hangfire: No se encontraron pagos pendientes para reconciliar.");
                return;
            }

            _logger.LogInformation($"Hangfire: Se encontraron {pendingPayments.Count()} pagos pendientes para reconciliar.");

            foreach (var payment in pendingPayments)
            {
                try 
                {
                    var stripeStatus = await _gateway.GetPaymentIntentStatusAsync(payment.StripePaymentIntentId!);
                    _logger.LogInformation($"Hangfire: Reconciliando pago {payment.Id}. Stripe Status: {stripeStatus}");

                    if (stripeStatus == "succeeded")
                    {
                        await _mediator.Send(new CapturePaymentCommand(payment.StripePaymentIntentId!), CancellationToken.None);
                        _logger.LogInformation($"Hangfire: Pago {payment.Id} correctamente.");
                    }
                    else if (stripeStatus == "canceled")
                    {
                        await _mediator.Send(new FailPaymentCommand(payment.StripePaymentIntentId!, "Cancelled on Gateway via Hangfire"), CancellationToken.None);
                        _logger.LogInformation($"Hangfire: Pago {payment.Id} cancelado.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Hangfire: Error al reconciliar pago {payment.Id}");
                }
            }
        }
    }
}
