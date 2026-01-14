using System;
using System.Collections.Generic;

namespace SeatingMS.Shared.Dtos
{
    public class ValidateLockRequestDto
    {
        public List<Guid> SeatIds { get; set; }
        public Guid UserId { get; set; }
    }
}
