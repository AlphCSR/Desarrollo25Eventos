using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MarketingMS.Application.Interfaces
{
    public interface IEventsService
    {
        Task<EventDetailsDto?> GetEventDetailsAsync(Guid eventId);
        Task<IEnumerable<EventDetailsDto>> GetEventsAsync(string? category, bool futureOnly);
    }

    public class EventDetailsDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public List<string> Categories { get; set; } = new();
        public DateTime Date { get; set; }
        public string ImageUrl { get; set; }
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }

        public PagedResult() { }
        public PagedResult(List<T> items, int totalCount, int page, int pageSize)
        {
            Items = items;
            TotalCount = totalCount;
            Page = page;
            PageSize = pageSize;
        }
    }
}
