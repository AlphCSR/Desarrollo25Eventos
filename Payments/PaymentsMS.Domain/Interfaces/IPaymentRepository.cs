using PaymentsMS.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PaymentsMS.Domain.Interfaces
{
    public interface IPaymentRepository
    {
        Task AddAsync(Payment payment, CancellationToken cancellationToken);
        Task<Payment?> GetByPaymentIntentIdAsync(string paymentIntentId, CancellationToken cancellationToken);
        Task<Payment?> GetByBookingIdAsync(Guid bookingId);
        Task<System.Collections.Generic.IEnumerable<Payment>> GetPendingPaymentsOlderThanAsync(DateTime date, CancellationToken cancellationToken);
        Task UpdateAsync(Payment payment, CancellationToken cancellationToken);
        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}
