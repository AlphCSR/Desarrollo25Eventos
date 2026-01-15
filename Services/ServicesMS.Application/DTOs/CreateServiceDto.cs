using System;

namespace ServicesMS.Application.DTOs
{
    public record CreateServiceDto(
        string Name,
        string Description,
        decimal BasePrice,
        Guid EventId,
        bool RequiresStock,
        int Stock
    );
}
