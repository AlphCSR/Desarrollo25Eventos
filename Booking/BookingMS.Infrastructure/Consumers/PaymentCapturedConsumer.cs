using MassTransit;
using Microsoft.Extensions.Logging;
using BookingMS.Shared.Events;
using BookingMS.Domain.Interfaces;
using BookingMS.Domain.Entities;
using BookingMS.Shared.Enums;
using System.Threading.Tasks;

namespace BookingMS.Infrastructure.Consumers
{
    public class PaymentCapturedConsumer : IConsumer<PaymentCapturedEvent>
    {
        private readonly IBookingRepository _repository;
        private readonly ILogger<PaymentCapturedConsumer> _logger;
        // Necesitamos publicar que la reserva se confirmó para que SeatingMS se entere
        // IPublishEndpoint viene directo de MassTransit
        
        public PaymentCapturedConsumer(IBookingRepository repository, ILogger<PaymentCapturedConsumer> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<PaymentCapturedEvent> context)
        {
            var bookingId = context.Message.BookingId;
            var booking = await _repository.GetByIdAsync(bookingId, context.CancellationToken);

            if (booking == null)
            {
                _logger.LogError($"Reserva {bookingId} no encontrada para confirmar pago.");
                return;
            }

            if (booking.Status == BookingStatus.PendingPayment)
            {
                booking.ConfirmPayment();
                await _repository.SaveChangesAsync(context.CancellationToken);
                
                _logger.LogInformation($"Reserva {bookingId} CONFIRMADA. Pago: {context.Message.TransactionId}");

                // Avisar a otros microservicios (SeatingMS, NotificationsMS)
                await context.Publish(new BookingConfirmedEvent 
                { 
                    BookingId = booking.Id,
                    UserId = booking.UserId,
                    Email = "user@example.com" // Debería venir persistido o del evento anterior
                });
            }
        }
    }
}