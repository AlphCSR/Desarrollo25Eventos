using MediatR;
using SeatingMS.Application.Commands;
using SeatingMS.Domain.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MassTransit;
using SeatingMS.Domain.Exceptions;
using SeatingMS.Shared.Events;

namespace SeatingMS.Application.Commands.LockSeat
{
    public class LockSeatCommandHandler : IRequestHandler<LockSeatCommand, bool>
    {
        private readonly IEventSeatRepository _repository;
        private readonly IPublishEndpoint _publishEndpoint;

        public LockSeatCommandHandler(IEventSeatRepository repository, IPublishEndpoint publishEndpoint)
        {
            _repository = repository;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<bool> Handle(LockSeatCommand request, CancellationToken cancellationToken)
        {
            var seat = await _repository.GetByIdAsync(request.SeatId, cancellationToken);
            
            if (seat == null) 
                throw new SeatNotFoundException(request.SeatId);

            seat.Lock(request.UserId);
            
            try
            {
                await _repository.UpdateAsync(seat, cancellationToken);
                await _repository.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }

            await _publishEndpoint.Publish(new SeatStatusUpdatedEvent
            {
                SeatId = seat.Id,
                EventId = seat.EventId,
                Status = seat.Status,
                UserId = seat.CurrentUserId
            }, cancellationToken);

            return true;
        }
    }
}