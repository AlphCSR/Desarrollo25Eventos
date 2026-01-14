using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

using BookingMS.Application.Interfaces;
using BookingMS.Application.DTOs;

namespace BookingMS.Infrastructure.Services
{
    public class SeatingService : ISeatingService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SeatingService> _logger;
        private readonly string _seatingUrl;

        public SeatingService(HttpClient httpClient, ILogger<SeatingService> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _seatingUrl = configuration["Seating:SeatingUrl"] ?? "http://localhost:5003";
        }

        public async Task<bool> ValidateLockAsync(List<Guid> seatIds, Guid userId, CancellationToken cancellationToken)
        {
            try
            {
                var payload = new 
                {
                    SeatIds = seatIds,
                    UserId = userId
                };

                var response = await _httpClient.PostAsJsonAsync($"{_seatingUrl}/api/Seating/validate-lock", payload, cancellationToken);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Validacion de asiento fallida con codigo de estado {StatusCode}", response.StatusCode);
                    return false;
                }

                return await response.Content.ReadFromJsonAsync<bool>(cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar el asiento con SeatingMS");
                return false;
            }
        }

        public async Task<SeatDetailDto> GetSeatDetailAsync(Guid seatId, CancellationToken cancellationToken)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<SeatDetailDto>($"{_seatingUrl}/api/Seating/{seatId}", cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el detalle del asiento para {SeatId}", seatId);
                return null;
            }
        }
    }
}
