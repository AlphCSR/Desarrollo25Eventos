using System;

namespace BookingMS.Shared.Dtos.Response;

public record InvoiceItemDto(
    string Description,
    decimal UnitPrice,
    int Quantity,
    decimal Total
);
