using MediatR;
using MarketingMS.Application.Interfaces;
using MarketingMS.Domain.Interfaces;

namespace MarketingMS.Application.Queries.GetRecommendations
{
    public class GetRecommendationsQueryHandler : IRequestHandler<GetRecommendationsQuery, IEnumerable<EventDetailsDto>>
    {
        private readonly IUserInterestRepository _repository;
        private readonly IEventsService _eventsService;

        public GetRecommendationsQueryHandler(IUserInterestRepository repository, IEventsService eventsService)
        {
            _repository = repository;
            _eventsService = eventsService;
        }

        public async Task<IEnumerable<EventDetailsDto>> Handle(GetRecommendationsQuery request, CancellationToken cancellationToken)
        {
            var topCategoriesList = (await _repository.GetTopCategoriesAsync(request.UserId, 5)).ToList();
            
            Console.WriteLine($"[MarketingMS] Obteniendo categorias para {request.UserId}: {string.Join(", ", topCategoriesList)}");

            if (!topCategoriesList.Any())
            {
                Console.WriteLine("[MarketingMS] No se encontraron interests, Obteniendo eventos generales.");
                return await _eventsService.GetEventsAsync(null, true);
            }

            var tasks = topCategoriesList.Select(cat => _eventsService.GetEventsAsync(cat, true));
            var resultsArray = await Task.WhenAll(tasks);
            
            var categoryMap = topCategoriesList
                .Zip(resultsArray, (cat, events) => new { Category = cat, Events = events.ToList() })
                .ToList();
            var finalRecommendations = new List<EventDetailsDto>();
            var seenIds = new HashSet<Guid>();

            for (int i = 0; i < 2; i++) 
            {
                foreach (var entry in categoryMap)
                {
                    if (finalRecommendations.Count >= 6) break;
                    
                    var ev = entry.Events.FirstOrDefault(e => !seenIds.Contains(e.Id));
                    if (ev != null)
                    {
                        finalRecommendations.Add(ev);
                        seenIds.Add(ev.Id);
                        entry.Events.Remove(ev);
                    }
                }
            }

            if (finalRecommendations.Count < 3)
            {
                Console.WriteLine("[MarketingMS] No se encontraron suficientes recomendaciones, Obteniendo eventos generales.");
                var generalEvents = await _eventsService.GetEventsAsync(null, true);
                var generalToAdd = generalEvents
                    .Where(e => !seenIds.Contains(e.Id))
                    .Take(6 - finalRecommendations.Count);
                
                finalRecommendations.AddRange(generalToAdd);
            }
            
            return finalRecommendations.Take(6);
        }
    }
}
