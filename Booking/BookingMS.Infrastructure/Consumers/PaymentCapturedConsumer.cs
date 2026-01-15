using MassTransit;
using Microsoft.Extensions.Logging;
using BookingMS.Shared.Events;
using BookingMS.Domain.Interfaces;
using BookingMS.Domain.Entities;
using BookingMS.Shared.Enums;
using BookingMS.Application.Interfaces;
using BookingMS.Shared.Dtos.Response;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookingMS.Application.DTOs;

namespace BookingMS.Infrastructure.Consumers
{
    public class PaymentCapturedConsumer : IConsumer<PaymentCapturedEvent>
    {
        private readonly IBookingRepository _repository;
        private readonly ILogger<PaymentCapturedConsumer> _logger;
        private readonly ISeatingService _seatingService;
        private readonly IServicesService _servicesService;
        
        public PaymentCapturedConsumer(
            IBookingRepository repository, 
            ILogger<PaymentCapturedConsumer> logger,
            ISeatingService seatingService,
            IServicesService servicesService)
        {
            _repository = repository;
            _logger = logger;
            _seatingService = seatingService;
            _servicesService = servicesService;
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
                
                _logger.LogInformation($"Reserva {bookingId} confirmada. Pago: {context.Message.TransactionId}");

                var items = new List<InvoiceItemDto>();
                decimal servicesTotal = 0;
                var serviceDetails = new List<ServiceDetailDto>();

                foreach (var serviceId in booking.ServiceIds)
                {
                    var service = await _servicesService.GetServiceDetailAsync(serviceId, context.CancellationToken);
                    if (service != null)
                    {
                        serviceDetails.Add(service);
                        servicesTotal += service.Price;
                    }
                }

                decimal seatsTotal = booking.TotalAmount - servicesTotal;
                decimal seatUnitPrice = booking.SeatIds.Count > 0 ? seatsTotal / booking.SeatIds.Count : 0;

                foreach (var seatId in booking.SeatIds)
                {
                    var seat = await _seatingService.GetSeatDetailAsync(seatId, context.CancellationToken);
                    if (seat != null)
                    {
                        items.Add(new InvoiceItemDto($"Entrada - Fila {seat.Row}, Asiento {seat.Number}", seatUnitPrice, 1, seatUnitPrice));
                    }
                }

                foreach (var service in serviceDetails)
                {
                    items.Add(new InvoiceItemDto($"Servicio - {service.Name}", service.Price, 1, service.Price));
                }

                await context.Publish(new BookingConfirmedEvent 
                { 
                    BookingId = booking.Id,
                    UserId = booking.UserId,
                    EventId = booking.EventId,
                    TotalAmount = booking.TotalAmount,
                    SeatIds = booking.SeatIds.ToList(),
                    ServiceIds = booking.ServiceIds.ToList(),
                    Items = items,
                    ConfirmedAt = System.DateTime.UtcNow,
                    Email = booking.Email,
                    CouponCode = booking.CouponCode,
                    DiscountAmount = booking.DiscountAmount
                });
            }
        }
    }
}
