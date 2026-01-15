using BookingMS.Application.Interfaces;
using BookingMS.Application.DTOs;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace BookingMS.Infrastructure.Services
{
    public class ServicesService : IServicesService
    {
        private readonly HttpClient _httpClient;
        private readonly string _servicesUrl;
        private readonly ILogger<ServicesService> _logger;

        public ServicesService(HttpClient httpClient, IConfiguration configuration, ILogger<ServicesService> logger)
        {
            _httpClient = httpClient;
            _servicesUrl = configuration["Services:ServicesUrl"] ?? "http://localhost:5006";
            _logger = logger;
        }

        public async Task<bool> BookServiceAsync(Guid serviceId, Guid userId, Guid bookingId, int quantity, CancellationToken cancellationToken)
        {
            try 
            {
                var payload = new 
                { 
                    ServiceId = serviceId, 
                    UserId = userId, 
                    BookingId = bookingId, 
                    Quantity = quantity 
                };

                var response = await _httpClient.PostAsJsonAsync($"{_servicesUrl}/api/services/book", payload, cancellationToken);
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error llamando a ServicesMS");
                return false;
            }
        }

        public async Task<ServiceDetailDto> GetServiceDetailAsync(Guid serviceId, CancellationToken cancellationToken)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<ServiceDetailDto>($"{_servicesUrl}/api/services/{serviceId}", cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo el detalle del servicio para {ServiceId}", serviceId);
                return null;
            }
        }
    }
}
