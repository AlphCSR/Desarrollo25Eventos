using MediatR;
using System.Collections.Generic;
using BookingMS.Domain.Entities;

namespace BookingMS.Application.Queries
{
    public class GetAuditLogsQuery : IRequest<IEnumerable<AuditLog>>
    {
        public int Count { get; set; } = 50;
    }
}
