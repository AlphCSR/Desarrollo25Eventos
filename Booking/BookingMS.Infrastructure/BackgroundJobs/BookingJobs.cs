using BookingMS.Domain.Interfaces;
using BookingMS.Shared.Events;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace BookingMS.Infrastructure.BackgroundJobs
{
    [ExcludeFromCodeCoverage]
    public class BookingJobs
    {
        private readonly IBookingRepository _repository;
        private readonly IEventPublisher _publisher;
        private readonly ILogger<BookingJobs> _logger;

        public BookingJobs(IBookingRepository repository, IEventPublisher publisher, ILogger<BookingJobs> logger)
        {
            _repository = repository;
            _publisher = publisher;
            _logger = logger;
        }

        public async Task ProcessExpiredBookingsAsync()
        {
            _logger.LogInformation("Hangfire: Buscando reservas expiradas...");
            
            var cutoff = DateTime.UtcNow.AddMinutes(-5); 
            var expiredBookings = await _repository.GetExpiredBookingsAsync(cutoff, CancellationToken.None);

            if (expiredBookings != null && expiredBookings.Any())
            {
                _logger.LogInformation($"Hangfire: Se encontraron {expiredBookings.Count()} reservas expiradas. Cancelando...");

                foreach (var booking in expiredBookings)
                {
                    booking.Cancel("Expirada via Hangfire");
                    await _repository.UpdateAsync(booking);
                    
                    var evt = new BookingCancelledEvent
                    {
                        BookingId = booking.Id,
                        UserId = booking.UserId,
                        SeatIds = booking.SeatIds.ToList(),
                        Reason = "Expirada via Hangfire",
                        Email = booking.Email
                    };
                    
                    await _publisher.PublishAsync(evt, CancellationToken.None);
                }
                
                await _repository.SaveChangesAsync(CancellationToken.None);
            }
        }

        public async Task ProcessPaymentRemindersAsync()
        {
            _logger.LogInformation("Hangfire: Buscando reservas que necesitan recordatorio de pago...");
            
            var reminderThreshold = DateTime.UtcNow.AddMinutes(-3);
            var needingReminder = await _repository.GetBookingsNeedingReminderAsync(reminderThreshold, CancellationToken.None);
            
            if (needingReminder != null && needingReminder.Any())
            {
                _logger.LogInformation($"Hangfire: Se encontraron {needingReminder.Count()} reservas que necesitan recordatorio de pago. Enviando...");
                foreach (var booking in needingReminder)
                {
                    var reminderEvt = new PaymentExpiringSoonEvent
                    {
                        BookingId = booking.Id,
                        UserId = booking.UserId,
                        Email = booking.Email,
                        MinutesRemaining = 2
                    };
                    
                    await _publisher.PublishAsync(reminderEvt, CancellationToken.None);
                    booking.MarkReminderSent();
                    await _repository.UpdateAsync(booking);
                }
                await _repository.SaveChangesAsync(CancellationToken.None);
            }
        }
    }
}
