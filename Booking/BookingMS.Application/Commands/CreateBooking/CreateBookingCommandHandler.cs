using MediatR;
using BookingMS.Application.Commands;
using BookingMS.Domain.Interfaces;
using BookingMS.Shared.Dtos.Response;
using BookingMS.Domain.Entities;
using BookingMS.Shared.Events;
using BookingMS.Application.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace BookingMS.Application.Commands.CreateBooking
{
    public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, BookingDto>
    {
        private readonly IBookingRepository _repository;
        private readonly IEventPublisher _publisher;
        private readonly IMarketingService _marketingService;
        private readonly ISeatingService _seatingService;
        private readonly IServicesService _servicesService;

        public CreateBookingCommandHandler(IBookingRepository repository, IEventPublisher publisher, IMarketingService marketingService, ISeatingService seatingService, IServicesService servicesService)
        {
            _repository = repository;
            _publisher = publisher;
            _marketingService = marketingService;
            _seatingService = seatingService;
            _servicesService = servicesService;
        }

        public async Task<BookingDto> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
        {
            string? validCoupon = null;
            decimal discount = 0;

            if (request.SeatIds != null && request.SeatIds.Any())
            {
                var isLockedByUser = await _seatingService.ValidateLockAsync(request.SeatIds, request.UserId, cancellationToken);
                if (!isLockedByUser)
                {
                    throw new ValidationException("Verificación de seguridad fallida: No tienes el bloqueo estricto de estos asientos. Alguien más podría haberlos tomado o expiraron.");
                }
            }

            if (!string.IsNullOrWhiteSpace(request.CouponCode))
            {
                var validation = await _marketingService.ValidateCouponAsync(request.CouponCode, request.TotalAmount, cancellationToken);
                if (validation != null)
                {
                    validCoupon = validation.Code;
                    discount = validation.DiscountAmount;
                }
            }

            var booking = new Booking(
                request.UserId, 
                request.EventId, 
                request.SeatIds, 
                request.ServiceIds, 
                request.TotalAmount - discount, 
                request.UserEmail,
                validCoupon,
                discount
            );

            if (request.ServiceIds != null && request.ServiceIds.Any())
            {
                foreach (var serviceId in request.ServiceIds)
                {
                    var success = await _servicesService.BookServiceAsync(serviceId, request.UserId, booking.Id, 1, cancellationToken);
                    if (!success)
                    {
                         throw new ValidationException($"El servicio {serviceId} está agotado o no disponible.");
                    }
                }
            }

            await _repository.AddAsync(booking, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            var integrationEvent = new BookingCreatedEvent
            {
                BookingId = booking.Id,
                UserId = booking.UserId,
                TotalAmount = booking.TotalAmount, 
                Email = booking.Email
            };

            await _publisher.PublishAsync(integrationEvent, cancellationToken);

            return new BookingDto(
                booking.Id,
                booking.UserId,
                booking.EventId,
                booking.SeatIds.ToList(),
                booking.ServiceIds.ToList(),
                booking.TotalAmount,
                booking.Status,
                booking.CreatedAt,
                booking.CouponCode,
                booking.DiscountAmount
            );
        }
    }
}