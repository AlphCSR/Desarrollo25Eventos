using MediatR;
using System;
using System.Collections.Generic;

namespace SeatingMS.Application.Queries.ValidateLock
{
    public class ValidateLockQuery : IRequest<bool>
    {
        public List<Guid> SeatIds { get; set; }
        public Guid UserId { get; set; }

        public ValidateLockQuery(List<Guid> seatIds, Guid userId)
        {
            SeatIds = seatIds;
            UserId = userId;
        }
    }
}
