using MediatR;
using ReportsMS.Application.DTOs;
using System;

namespace ReportsMS.Application.Queries
{
    public record GetSalesReportQuery(Guid EventId) : IRequest<SalesReportDto>;
}
