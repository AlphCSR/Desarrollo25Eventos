using System;

namespace ReportsMS.Application.DTOs
{
    public record SalesReportDto(Guid EventId, int TotalBookings, decimal TotalRevenue);

    public record EventDetailedReportDto(
        Guid EventId,
        int TotalCapacity,
        int SoldSeats,
        decimal TotalRevenue,
        IEnumerable<AttendeeDto> Attendees,
        IEnumerable<DailySaleDto> DailySales
    );

    public record AttendeeDto(string Email, DateTime Date, decimal Amount);
    public record DailySaleDto(DateTime Date, decimal Amount, int Count);
}
