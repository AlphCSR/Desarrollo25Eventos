using MassTransit;
using Microsoft.Extensions.Logging;
using BookingMS.Shared.Events;
using BookingMS.Domain.Interfaces;
using BookingMS.Shared.Enums;
using System.Threading.Tasks;

namespace BookingMS.Infrastructure.Consumers
{
    public class PaymentFailedConsumer : IConsumer<PaymentFailedEvent>
    {
        private readonly IBookingRepository _repository;
        private readonly ILogger<PaymentFailedConsumer> _logger;

        public PaymentFailedConsumer(IBookingRepository repository, ILogger<PaymentFailedConsumer> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
        {
            var bookingId = context.Message.BookingId;
            var booking = await _repository.GetByIdAsync(bookingId, context.CancellationToken);

            if (booking != null && booking.Status == BookingStatus.PendingPayment)
            {
                booking.Cancel($"Pago fallido: {context.Message.Reason}");
                await _repository.SaveChangesAsync(context.CancellationToken);
                
                _logger.LogWarning($"Reserva {bookingId} CANCELADA por fallo en pago.");

                await context.Publish(new BookingCancelledEvent 
                { 
                    BookingId = booking.Id,
                    UserId = booking.UserId,
                    SeatIds = booking.SeatIds.ToList(),
                    Reason = context.Message.Reason,
                    Email = booking.Email
                });
            }
        }
    }
}