using MediatR;
using BookingMS.Application.Commands;
using BookingMS.Domain.Interfaces;
using BookingMS.Shared.Dtos.Response;
using BookingMS.Domain.Entities;
using BookingMS.Shared.Events;

using System.Threading;
using System.Threading.Tasks;

namespace BookingMS.Application.Commands.CreateBooking
{
    public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, BookingDto>
    {
        private readonly IBookingRepository _repository;
        private readonly IEventPublisher _publisher;

        public CreateBookingCommandHandler(IBookingRepository repository, IEventPublisher publisher)
        {
            _repository = repository;
            _publisher = publisher;
        }

        public async Task<BookingDto> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
        {
            // 1. Crear la entidad (Lógica de dominio)
            var booking = new Booking(request.UserId, request.EventId, request.SeatIds, request.TotalAmount);

            // 2. Persistir en Postgres
            await _repository.AddAsync(booking, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            // 3. Publicar Evento de Integración a RabbitMQ
            var integrationEvent = new BookingCreatedEvent
            {
                BookingId = booking.Id,
                UserId = booking.UserId,
                TotalAmount = booking.TotalAmount,
                Email = "user@example.com"
            };

            await _publisher.PublishAsync(integrationEvent, cancellationToken);

            // 4. Retornar DTO
            // 4. Retornar DTO
            return new BookingDto(
                booking.Id,
                booking.UserId,
                booking.EventId,
                booking.SeatIds.ToList(),
                booking.TotalAmount,
                booking.Status,
                booking.CreatedAt
            );
        }
    }
}