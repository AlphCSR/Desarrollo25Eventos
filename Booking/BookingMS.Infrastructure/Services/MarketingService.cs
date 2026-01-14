using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using BookingMS.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BookingMS.Infrastructure.Services
{
    public class MarketingService : IMarketingService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly ILogger<MarketingService> _logger;

        public MarketingService(HttpClient httpClient, IConfiguration configuration, ILogger<MarketingService> logger)
        {
            _httpClient = httpClient;
            _baseUrl = (configuration["Services:MarketingApi"] ?? "http://marketing-ms:8080").TrimEnd('/');
            _logger = logger;
        }

        public async Task<CouponValidationResult?> ValidateCouponAsync(string code, decimal amount, CancellationToken cancellationToken = default)
        {
            try
            {
                var amountStr = amount.ToString(CultureInfo.InvariantCulture);
                var url = $"{_baseUrl}/api/coupons/validate?code={code.Trim()}&amount={amountStr}";
                
                _logger.LogInformation("[MarketingService] Validando cupon {Code} por monto {Amount} en URL: {Url}", code, amount, url);

                var response = await _httpClient.GetAsync(url, cancellationToken);
                
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("[MarketingService] Validacion fallida. Status: {Status}, Error: {Error}", response.StatusCode, error);
                    return null;
                }

                var coupon = await response.Content.ReadFromJsonAsync<CouponValidationResponse>(cancellationToken: cancellationToken);
                
                if (coupon == null) return null;

                _logger.LogInformation("[MarketingService] Cupon recibido: {Code}, Tipo: {Type}, Valor: {Value}", coupon.Code, coupon.Type, coupon.Value);

                decimal discount = 0;
                var typeStr = coupon.Type?.ToString() ?? "0";
                if (typeStr.Equals("Percentage", StringComparison.OrdinalIgnoreCase) || typeStr == "0")
                {
                    discount = amount * (coupon.Value / 100);
                }
                else
                {
                    discount = coupon.Value;
                }

                if (discount > amount) discount = amount;

                _logger.LogInformation("[MarketingService] Calculado descuento: {Discount}", discount);

                return new CouponValidationResult
                {
                    Code = coupon.Code,
                    Type = typeStr,
                    Value = coupon.Value,
                    DiscountAmount = discount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MarketingService] Error critico al comunicarse con MarketingMS");
                return null;
            }
        }

        private class CouponValidationResponse
        {
            public string Code { get; set; } = string.Empty;
            public object Type { get; set; } = 0;
            public decimal Value { get; set; }
        }
    }
}
