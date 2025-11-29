using System;

using System.Diagnostics.CodeAnalysis;

namespace SeatingMS.Application.DTOs
{
    [ExcludeFromCodeCoverage]
    public record LockSeatRequestDto(Guid SeatId, Guid UserId);
}