using System;

using System.Diagnostics.CodeAnalysis;

namespace SeatingMS.Application.DTOs
{
    [ExcludeFromCodeCoverage]
    public record SeatDto(Guid Id, string Row, int Number, string Status, decimal? Price, Guid? UserId, Guid SectionId);
}
