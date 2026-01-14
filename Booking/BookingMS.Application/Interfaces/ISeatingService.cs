using BookingMS.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BookingMS.Application.Interfaces
{
    public interface ISeatingService
    {
        Task<bool> ValidateLockAsync(List<Guid> seatIds, Guid userId, CancellationToken cancellationToken);
        Task<SeatDetailDto> GetSeatDetailAsync(Guid seatId, CancellationToken cancellationToken);
    }
}
