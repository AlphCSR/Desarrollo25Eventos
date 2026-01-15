using System.Threading.Tasks;

namespace PaymentsMS.Core.Service
{
    public interface IPaymentGateway
    {
        Task<string> CreateCustomer(string email, string name);
        Task<bool> ProcessPayment(string customerId, string paymentMethodId, long amountInCents, string currency);
    }
}
