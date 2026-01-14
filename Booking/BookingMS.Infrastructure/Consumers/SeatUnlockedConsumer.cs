using MassTransit;
using BookingMS.Domain.Interfaces;
using SeatingMS.Shared.Events;
using System.Threading.Tasks;
using System.Linq;

namespace BookingMS.Infrastructure.Consumers
{
    public class SeatUnlockedConsumer : IConsumer<SeatUnlockedEvent>
    {
        private readonly IBookingRepository _repository;

        public SeatUnlockedConsumer(IBookingRepository repository)
        {
            _repository = repository;
        }

        public async Task Consume(ConsumeContext<SeatUnlockedEvent> context)
        {
            var message = context.Message;
            
            var bookings = await _repository.GetByUserIdAsync(message.UserId);
            var activeBooking = bookings.FirstOrDefault(b => b.Status == BookingMS.Shared.Enums.BookingStatus.PendingPayment && b.SeatIds.Contains(message.SeatId));

            if (activeBooking != null)
            {
                activeBooking.RemoveSeat(message.SeatId);
                await _repository.UpdateAsync(activeBooking);
                await _repository.SaveChangesAsync(context.CancellationToken);
            }
        }
    }
}
