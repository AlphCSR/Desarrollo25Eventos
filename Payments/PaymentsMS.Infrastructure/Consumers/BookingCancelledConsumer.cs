using BookingMS.Shared.Events;
using MassTransit;
using PaymentsMS.Domain.Interfaces;
using System.Threading.Tasks;

namespace PaymentsMS.Infrastructure.Consumers;

public class BookingCancelledConsumer : IConsumer<BookingCancelledEvent>
{
    private readonly IPaymentRepository _repository;
    private readonly IPaymentGateway _gateway;

    public BookingCancelledConsumer(IPaymentRepository repository, IPaymentGateway gateway)
    {
        _repository = repository;
        _gateway = gateway;
    }

    public async Task Consume(ConsumeContext<BookingCancelledEvent> context)
    {
        var bookingId = context.Message.BookingId;

        var payment = await _repository.GetByBookingIdAsync(bookingId);
        
        if (payment != null && payment.Status == "Succeeded" && !string.IsNullOrEmpty(payment.StripePaymentIntentId))
        {
            var success = await _gateway.RefundAsync(payment.StripePaymentIntentId);
            
            if (success)
            {
                payment.MarkAsRefunded();
                await _repository.UpdateAsync(payment, context.CancellationToken);
                await _repository.SaveChangesAsync(context.CancellationToken);
                
                System.Console.WriteLine($"[Refund] Reserva {bookingId} reembolsada.");
            }
        }
    }
}
