using MediatR;
using SeatingMS.Domain.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using SeatingMS.Shared.Events;
using Microsoft.Extensions.Logging;

namespace SeatingMS.Application.Commands.UnlockSeat
{
    public class UnlockSeatCommandHandler : IRequestHandler<UnlockSeatCommand, bool>
    {
        private readonly IEventSeatRepository _repository;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<UnlockSeatCommandHandler> _logger;

        public UnlockSeatCommandHandler(IEventSeatRepository repository, IPublishEndpoint publishEndpoint, ILogger<UnlockSeatCommandHandler> logger)
        {
            _repository = repository;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task<bool> Handle(UnlockSeatCommand request, CancellationToken cancellationToken)
        {
            var seat = await _repository.GetByIdAsync(request.SeatId, cancellationToken);

            if (seat == null)
            {
                _logger.LogWarning($"[UnlockSeat] Seat not found: {request.SeatId}");
                throw new Exception("Asiento no encontrado");
            }

            if (seat.CurrentUserId != Guid.Empty && seat.CurrentUserId != null && seat.CurrentUserId != request.UserId)
            {
                 _logger.LogWarning($"[SeatingMS] Security Alert: UnlockSeat User mismatch. SeatUser: {seat.CurrentUserId}, RequestUser: {request.UserId}. Status: {seat.Status}. Releasing anyway to prevent deadlock.");
            }

            var previousStatus = seat.Status;
            seat.Release();

            await _repository.UpdateAsync(seat, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation($"[SeatingMS] Seat {seat.Id} unlocked. Previous Status: {previousStatus}. Original User: {seat.CurrentUserId}");
            
            await _publishEndpoint.Publish(new SeatUnlockedEvent(seat.Id, request.UserId, DateTime.UtcNow), cancellationToken);

            await _publishEndpoint.Publish(new SeatStatusUpdatedEvent
            {
                SeatId = seat.Id,
                EventId = seat.EventId,
                Status = seat.Status,
                UserId = null
            }, cancellationToken);

            return true;
        }
    }
}
