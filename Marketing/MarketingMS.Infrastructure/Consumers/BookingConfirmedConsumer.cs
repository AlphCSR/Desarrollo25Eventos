using MassTransit;
using BookingMS.Shared.Events;
using MarketingMS.Application.Interfaces;
using MarketingMS.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace MarketingMS.Infrastructure.Consumers
{
    public class BookingConfirmedConsumer : IConsumer<BookingConfirmedEvent>
    {
        private readonly IUserInterestRepository _interestRepository;
        private readonly ICouponRepository _couponRepository;
        private readonly IEventsService _eventsService;
        private readonly ILogger<BookingConfirmedConsumer> _logger;

        public BookingConfirmedConsumer(IUserInterestRepository interestRepository, ICouponRepository couponRepository, IEventsService eventsService, ILogger<BookingConfirmedConsumer> logger)
        {
            _interestRepository = interestRepository;
            _couponRepository = couponRepository;
            _eventsService = eventsService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<BookingConfirmedEvent> context)
        {
            var evt = context.Message;
            _logger.LogInformation("Procesando booking {BookingId} para el usuario {UserId}", evt.BookingId, evt.UserId);

            var eventDetails = await _eventsService.GetEventDetailsAsync(evt.EventId);
            
            if (eventDetails != null && eventDetails.Categories.Any())
            {
                foreach (var category in eventDetails.Categories)
                {
                    await _interestRepository.UpdateInterestAsync(evt.UserId, category);
                }
                _logger.LogInformation("Intereses actualizados para el usuario {UserId}: {Categories}", evt.UserId, string.Join(", ", eventDetails.Categories));
            }

            if (!string.IsNullOrWhiteSpace(evt.CouponCode))
            {
                var coupon = await _couponRepository.GetByCodeAsync(evt.CouponCode, context.CancellationToken);
                if (coupon != null)
                {
                    coupon.IncrementUsage();
                    await _couponRepository.UpdateAsync(coupon, context.CancellationToken);
                    await _couponRepository.SaveChangesAsync(context.CancellationToken);
                    _logger.LogInformation("Incrementado el uso del cupon {CouponCode}. Nuevo conteo: {CurrentUsage}", evt.CouponCode, coupon.CurrentUsage);
                }
            }
        }
    }
}
