using MediatR;
using SeatingMS.Application.Commands;
using SeatingMS.Domain.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SeatingMS.Application.Commands.LockSeat
{
    public class LockSeatCommandHandler : IRequestHandler<LockSeatCommand, bool>
    {
        private readonly IEventSeatRepository _repository;
        public LockSeatCommandHandler(IEventSeatRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(LockSeatCommand request, CancellationToken cancellationToken)
        {
            var seat = await _repository.GetByIdAsync(request.SeatId, cancellationToken);
            
            if (seat == null) 
                throw new Domain.Exceptions.SeatNotFoundException(request.SeatId);

            seat.Lock(request.UserId);
            
            await _repository.UpdateAsync(seat, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}