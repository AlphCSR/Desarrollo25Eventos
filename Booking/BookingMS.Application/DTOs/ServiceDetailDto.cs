using System;

namespace BookingMS.Application.DTOs;

public record ServiceDetailDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price
);
