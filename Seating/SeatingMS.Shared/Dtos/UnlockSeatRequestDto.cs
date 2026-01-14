using System;

using System.Diagnostics.CodeAnalysis;

namespace SeatingMS.Shared.Dtos
{
    [ExcludeFromCodeCoverage]
    public class UnlockSeatRequestDto
    {
        public Guid SeatId { get; set; }
        public Guid UserId { get; set; }
    }
}
