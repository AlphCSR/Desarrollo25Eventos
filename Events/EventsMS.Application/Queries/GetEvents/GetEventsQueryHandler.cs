using MediatR;
using EventsMS.Application.DTOs;
using EventsMS.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace EventsMS.Application.Queries.GetEvents
{
    public class GetEventsQueryHandler : IRequestHandler<GetEventsQuery, PagedResult<EventDto>>
    {
        private readonly IEventRepository _repository;

        public GetEventsQueryHandler(IEventRepository repository)
        {
            _repository = repository;
        }

        public async Task<PagedResult<EventDto>> Handle(GetEventsQuery request, CancellationToken cancellationToken)
        {
            if (request.Page < 1) request.Page = 1;
            if (request.PageSize < 1) request.PageSize = 10;

            var (items, totalCount) = await _repository.GetPagedAsync(
                request.Page, 
                request.PageSize, 
                request.Category, 
                request.StartDate, 
                request.EndDate, 
                request.MinPrice, 
                request.MaxPrice, 
                request.Type, 
                request.IncludeAll,
                request.FutureEventsOnly,
                cancellationToken);

            var dtos = items.Select(e => new EventDto
            {
                Id = e.Id,
                IdUser = e.IdUser,
                Title = e.Title,
                Description = e.Description,
                Date = e.Date,
                EndDate = e.EndDate,
                VenueName = e.VenueName,
                ImageUrl = e.ImageUrl,
                Status = e.Status,
                Type = e.Type,
                StreamingUrl = e.StreamingUrl,
                Categories = e.Categories,
                MinPrice = e.Sections?.Any() == true ? e.Sections.Min(s => s.Price) : 0
            }).ToList();

            return new PagedResult<EventDto>(dtos, totalCount, request.Page, request.PageSize);
        }
    }
}
