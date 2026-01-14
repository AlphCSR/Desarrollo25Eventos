using System;

namespace InvoicingMS.Shared.Dtos;

public record InvoiceItemInvoicingDto(
    string Description,
    decimal UnitPrice,
    int Quantity,
    decimal Total
);
