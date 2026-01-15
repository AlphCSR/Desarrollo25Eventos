using MediatR;
using SeatingMS.Domain.Interfaces;
using SeatingMS.Shared.Enum;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SeatingMS.Application.Queries.ValidateLock
{
    public class ValidateLockQueryHandler : IRequestHandler<ValidateLockQuery, bool>
    {
        private readonly IEventSeatRepository _repository;

        public ValidateLockQueryHandler(IEventSeatRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(ValidateLockQuery request, CancellationToken cancellationToken)
        {    
            foreach (var seatId in request.SeatIds)
            {
                var seat = await _repository.GetByIdAsync(seatId, cancellationToken);
                
                if (seat == null) return false; 
                
                if (seat.Status != SeatStatus.Locked) return false;
                if (seat.CurrentUserId != request.UserId) return false;
                
                if (seat.LockExpirationTime.HasValue && seat.LockExpirationTime.Value < System.DateTime.UtcNow) return false;
            }

            return true;
        }
    }
}
