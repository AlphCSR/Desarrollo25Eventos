using ServicesMS.Shared.Events;
using MassTransit;
using NotificationsMS.Domain.Interfaces;
using System.Threading.Tasks;

namespace NotificationsMS.Infrastructure.Consumers
{
    public class ServiceBookedConsumer : IConsumer<ServiceBookedEvent>
    {
        private readonly IEmailService _emailService;

        public ServiceBookedConsumer(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task Consume(ConsumeContext<ServiceBookedEvent> context)
        {
            var message = context.Message;
            var subject = "Servicio Adicional Contratado";
            var body = $"Has contratado exitosamente el servicio adicional. Cantidad: {message.Quantity}. Total: {message.TotalPrice:C}";

            await _emailService.SendEmailAsync("user@example.com", subject, body);
        }
    }
}
