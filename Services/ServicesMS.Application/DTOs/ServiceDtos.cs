using System;

namespace ServicesMS.Application.DTOs
{
    public record ServiceDefinitionDto(Guid Id, string Name, string Description, decimal Price, bool RequiresStock, int Stock);
    public record BookServiceDto(Guid ServiceId, Guid UserId, Guid BookingId, int Quantity);
}
