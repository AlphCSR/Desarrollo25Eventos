using MediatR;
using EventsMS.Application.DTOs;
using System.Collections.Generic;

namespace EventsMS.Application.Queries.GetEvents
{
    public class GetEventsQuery : IRequest<PagedResult<EventDto>>
    {
        public bool IncludeAll { get; set; } = false;
        public string? Category { get; set; }
        public bool FutureEventsOnly { get; set; } = false;
        
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 1000; 
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? Type { get; set; }
    }
}
