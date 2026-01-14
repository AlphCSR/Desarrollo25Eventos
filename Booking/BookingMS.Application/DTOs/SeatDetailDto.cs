using System;

namespace BookingMS.Application.DTOs;

public record SeatDetailDto(
    Guid Id,
    string Row,
    int Number
);
