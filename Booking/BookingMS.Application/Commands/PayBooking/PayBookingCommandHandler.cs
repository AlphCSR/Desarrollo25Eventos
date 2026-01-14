using MediatR;
using BookingMS.Domain.Interfaces;
using BookingMS.Domain.Entities;
using BookingMS.Shared.Enums;
using BookingMS.Domain.Exceptions;
using BookingMS.Shared.Events;
using BookingMS.Application.Interfaces;
using BookingMS.Shared.Dtos.Response;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using BookingMS.Application.DTOs;

namespace BookingMS.Application.Commands.PayBooking
{
    public class PayBookingCommandHandler : IRequestHandler<PayBookingCommand, bool>
    {
        private readonly IBookingRepository _repository;
        private readonly IEventPublisher _publisher;
        private readonly ILogger<PayBookingCommandHandler> _logger;
        private readonly ISeatingService _seatingService;
        private readonly IServicesService _servicesService;

        public PayBookingCommandHandler(
            IBookingRepository repository, 
            IEventPublisher publisher, 
            ILogger<PayBookingCommandHandler> logger,
            ISeatingService seatingService,
            IServicesService servicesService)
        {
            _repository = repository;
            _publisher = publisher;
            _logger = logger;
            _seatingService = seatingService;
            _servicesService = servicesService;
        }

        public async Task<bool> Handle(PayBookingCommand request, CancellationToken cancellationToken)
        {
            var booking = await _repository.GetByIdAsync(request.BookingId, cancellationToken);
            if (booking == null) 
                throw new BookingNotFoundException(request.BookingId);

            if (booking.Status == BookingStatus.Confirmed) return true;

            booking.ConfirmPayment();

            await _repository.UpdateAsync(booking);
            await _repository.SaveChangesAsync(cancellationToken);

            var items = new List<InvoiceItemDto>();
            decimal servicesTotal = 0;
            var serviceDetails = new List<ServiceDetailDto>();

            foreach (var serviceId in booking.ServiceIds)
            {
                var service = await _servicesService.GetServiceDetailAsync(serviceId, cancellationToken);
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
                var seat = await _seatingService.GetSeatDetailAsync(seatId, cancellationToken);
                if (seat != null)
                {
                    items.Add(new InvoiceItemDto($"Entrada - Fila {seat.Row}, Asiento {seat.Number}", seatUnitPrice, 1, seatUnitPrice));
                }
            }

            foreach (var service in serviceDetails)
            {
                items.Add(new InvoiceItemDto($"Servicio - {service.Name}", service.Price, 1, service.Price));
            }

            _logger.LogInformation($"Publicando evento BookingConfirmedEvent para la reserva {booking.Id}");
            await _publisher.PublishAsync(new BookingConfirmedEvent
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
                EventName = "Evento Confirmado", 
                CouponCode = booking.CouponCode,
                DiscountAmount = booking.DiscountAmount,
                Language = request.Language
            }, cancellationToken);

            return true;
        }
    }
}
