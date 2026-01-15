using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PaymentsMS.Application.Commands.CapturePayment;
using PaymentsMS.Application.Commands.FailPayment;
using PaymentsMS.Domain.Interfaces;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PaymentsMS.Infrastructure.BackgroundJobs
{
    public class ReconciliationWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ReconciliationWorker> _logger;

        public ReconciliationWorker(IServiceProvider serviceProvider, ILogger<ReconciliationWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker de reconciliación iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ReconcilePayments(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error ejecutando el Worker de reconciliación.");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task ReconcilePayments(CancellationToken stoppingToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
               var repository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();
               var gateway = scope.ServiceProvider.GetRequiredService<IPaymentGateway>();
               var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

               var cutoffDate = DateTime.UtcNow.AddMinutes(-2);
               var pendingPayments = await repository.GetPendingPaymentsOlderThanAsync(cutoffDate, stoppingToken);

               if (!pendingPayments.Any()) return;

               _logger.LogInformation($"Encontrados {pendingPayments.Count()} pagos pendientes para reconciliar.");

               foreach (var payment in pendingPayments)
               {
                   try 
                   {
                       var stripeStatus = await gateway.GetPaymentIntentStatusAsync(payment.StripePaymentIntentId!);
                       
                       _logger.LogInformation($"Reconciliando pago {payment.Id}. Estado de Stripe: {stripeStatus}");

                       if (stripeStatus == "succeeded")
                       {
                           await mediator.Send(new CapturePaymentCommand(payment.StripePaymentIntentId!), stoppingToken);
                           _logger.LogInformation($"Pago {payment.Id} corregido a Sucedido.");
                       }
                       else if (stripeStatus == "canceled")
                       {
                           await mediator.Send(new FailPaymentCommand(payment.StripePaymentIntentId!, "Cancelado en Gateway"), stoppingToken);
                           _logger.LogInformation($"Pago {payment.Id} corregido a Fallido.");
                       }
                   }
                   catch (Exception ex)
                   {
                       _logger.LogError(ex, $"Fallo al reconciliar el pago {payment.Id}");
                   }
               }
            }
        }
    }
}
