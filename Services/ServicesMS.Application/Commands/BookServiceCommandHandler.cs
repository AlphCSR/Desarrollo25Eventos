using MediatR;
using ServicesMS.Domain.Entities;
using ServicesMS.Domain.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using ServicesMS.Shared.Events;

namespace ServicesMS.Application.Commands
{
    public class BookServiceCommandHandler : IRequestHandler<BookServiceCommand, bool>
    {
        private readonly IServiceRepository _repository;
        private readonly IPublishEndpoint _publishEndpoint;

        public BookServiceCommandHandler(IServiceRepository repository, IPublishEndpoint publishEndpoint)
        {
            _repository = repository;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<bool> Handle(BookServiceCommand request, CancellationToken cancellationToken)
        {
            var data = request.BookingData;
            var service = await _repository.GetDefinitionByIdAsync(data.ServiceId, cancellationToken);
            
            if (service == null) throw new Exception("Servicio no encontrado");

            if (!service.CheckAvailability(data.Quantity))
                throw new Exception("No hay suficiente stock");

            service.ReduceStock(data.Quantity);
            await _repository.UpdateDefinitionAsync(service, cancellationToken);

            var booking = new ServiceBooking(service.Id, data.UserId, data.BookingId, data.Quantity, service.BasePrice);
            await _repository.AddBookingAsync(booking, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            await _publishEndpoint.Publish(new ServiceBookedEvent
            {
                ServiceBookingId = booking.Id,
                ServiceDefinitionId = booking.ServiceDefinitionId,
                UserId = booking.UserId,
                BookingId = booking.BookingId,
                Quantity = booking.Quantity,
                TotalPrice = booking.TotalPrice
            }, cancellationToken);

            return true;
        }
    }
}
