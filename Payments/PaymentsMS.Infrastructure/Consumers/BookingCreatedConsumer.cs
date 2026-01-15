using MassTransit;
using Microsoft.Extensions.Logging;
using BookingMS.Shared.Events;
using PaymentsMS.Domain.Interfaces;
using PaymentsMS.Domain.Exceptions;
using System;
using System.Threading.Tasks;

namespace PaymentsMS.Infrastructure.Consumers
{
    public class BookingCreatedConsumer : IConsumer<BookingCreatedEvent>
    {
        private readonly IPaymentGateway _paymentGateway;
        private readonly ILogger<BookingCreatedConsumer> _logger;

        public BookingCreatedConsumer(IPaymentGateway paymentGateway, ILogger<BookingCreatedConsumer> logger)
        {
            _paymentGateway = paymentGateway;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<BookingCreatedEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation($"[Payments] Reserva creada: {message.BookingId}. Esperando pago manual.");

            await Task.CompletedTask;
        }
    }
}