using BookingMS.Shared.Enums;
using System.Collections.Generic;

namespace BookingMS.Shared.Dtos.Response;

public record BookingDto(
    Guid Id,
    Guid UserId,
    Guid EventId,
    List<Guid> SeatIds,
    List<Guid> ServiceIds,
    decimal TotalAmount,
    BookingStatus Status,
    DateTime CreatedAt,
    string? CouponCode = null,
    decimal DiscountAmount = 0
);
