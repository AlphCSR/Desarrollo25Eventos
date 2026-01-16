using MarketingMS.Application.Interfaces;
using System.Text.Json;
using System.Net.Http.Json;

namespace MarketingMS.Infrastructure.Services
{
    public class EventsService : IEventsService
    {
        private readonly HttpClient _httpClient;

        public EventsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<EventDetailsDto?> GetEventDetailsAsync(Guid eventId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<EventDetailsDto>($"api/events/{eventId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MarketingMS] Error obteniendo detalles del evento {eventId}: {ex.Message}");
                return null;
            }
        }

        public async Task<IEnumerable<EventDetailsDto>> GetEventsAsync(string? category, bool futureOnly)
        {
            try
            {
                var query = "?";
                if (!string.IsNullOrEmpty(category)) query += $"category={category}&";
                if (futureOnly) query += "futureOnly=true";

                var result = await _httpClient.GetFromJsonAsync<PagedResult<EventDetailsDto>>($"api/events{query}");
                return result?.Items ?? new List<EventDetailsDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MarketingMS] Error obteniendo eventos: {ex.Message}");
                return new List<EventDetailsDto>();
            }
        }
    }
}
