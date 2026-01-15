using Microsoft.Extensions.Configuration;
using PaymentsMS.Domain.Interfaces;
using Stripe;
using System.Threading.Tasks;

namespace PaymentsMS.Infrastructure.Gateways
{
    public class StripePaymentGateway : IPaymentGateway
    {
        private readonly string _apiKey;

        public StripePaymentGateway(IConfiguration configuration)
        {
            _apiKey = configuration["Stripe:SecretKeySK"];
            StripeConfiguration.ApiKey = _apiKey;
        }

        public async Task<Domain.Models.GatewayPaymentIntent> CreatePaymentIntentAsync(decimal amount, string currency, string metadataBookingId, Guid userId)
        {
            var customerId = await GetOrCreateCustomerAsync(userId);

            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(amount * 100),
                Currency = currency,
                Customer = customerId,
                SetupFutureUsage = "off_session", 
                Metadata = new System.Collections.Generic.Dictionary<string, string>
                {
                    { "BookingId", metadataBookingId },
                    { "UserId", userId.ToString() }
                },
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                },
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            return new Domain.Models.GatewayPaymentIntent(paymentIntent.Id, paymentIntent.ClientSecret);
        }

        private async Task<string> GetOrCreateCustomerAsync(Guid userId)
        {
            var customerService = new CustomerService();
            var existingCustomers = await customerService.ListAsync(new CustomerListOptions 
            { 
                Limit = 1 
            });

            var searchOptions = new CustomerSearchOptions
            {
                Query = $"metadata['UserId']:'{userId}'",
            };
            var searchResult = await customerService.SearchAsync(searchOptions);

            if (searchResult.Data.Count > 0)
            {
                return searchResult.Data[0].Id;
            }

            var createOptions = new CustomerCreateOptions
            {
                Metadata = new System.Collections.Generic.Dictionary<string, string>
                {
                    { "UserId", userId.ToString() }
                },
                Description = $"User {userId}"
            };

            var customer = await customerService.CreateAsync(createOptions);
            return customer.Id;
        }

        public async Task<string> GetPaymentIntentStatusAsync(string paymentIntentId)
        {
             var service = new PaymentIntentService();
             var paymentIntent = await service.GetAsync(paymentIntentId);
             return paymentIntent.Status; 
        }

        public async Task<bool> RefundAsync(string paymentIntentId)
        {
            try
            {
                var options = new RefundCreateOptions
                {
                    PaymentIntent = paymentIntentId,
                };
                var service = new RefundService();
                var refund = await service.CreateAsync(options);
                return refund.Status == "succeeded" || refund.Status == "pending";
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Stripe Error al reembolsar: {ex.Message}");
                return false;
            }
        }
    }
}
