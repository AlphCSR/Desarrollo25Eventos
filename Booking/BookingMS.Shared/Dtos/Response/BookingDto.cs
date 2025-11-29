using BookingMS.Shared.Enums;
using System.Collections.Generic;

namespace BookingMS.Shared.Dtos.Response;

public record BookingDto(
    Guid Id,
    Guid UserId,
    Guid EventId,
    List<Guid> SeatIds,
    decimal TotalAmount,
    BookingStatus Status,
    DateTime CreatedAt
);
