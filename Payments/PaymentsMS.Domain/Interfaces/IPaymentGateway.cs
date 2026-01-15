using System.Threading;
using System.Threading.Tasks;

namespace PaymentsMS.Domain.Interfaces
{
    public interface IPaymentGateway
    {
        Task<Models.GatewayPaymentIntent> CreatePaymentIntentAsync(decimal amount, string currency, string metadataBookingId, Guid userId);
        Task<string> GetPaymentIntentStatusAsync(string paymentIntentId);
        Task<bool> RefundAsync(string paymentIntentId);
    }
}
