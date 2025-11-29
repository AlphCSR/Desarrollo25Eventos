using MediatR;
using SeatingMS.Domain.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SeatingMS.Application.Commands.UnlockSeat
{
    public class UnlockSeatCommandHandler : IRequestHandler<UnlockSeatCommand, bool>
    {
        private readonly IEventSeatRepository _repository;
        private readonly MassTransit.IPublishEndpoint _publishEndpoint;

        public UnlockSeatCommandHandler(IEventSeatRepository repository, MassTransit.IPublishEndpoint publishEndpoint)
        {
            _repository = repository;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<bool> Handle(UnlockSeatCommand request, CancellationToken cancellationToken)
        {
            var seat = await _repository.GetByIdAsync(request.SeatId, cancellationToken);

            if (seat == null)
                throw new Exception("Asiento no encontrado");

            // Validar que el usuario sea el dueño del bloqueo
            if (seat.CurrentUserId != request.UserId)
                throw new InvalidOperationException("El asiento no está bloqueado por este usuario.");

            seat.Release();

            await _repository.UpdateAsync(seat, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            
            await _publishEndpoint.Publish(new Shared.Events.SeatUnlockedEvent(seat.Id, request.UserId, DateTime.UtcNow), cancellationToken);

            return true;
        }
    }
}
