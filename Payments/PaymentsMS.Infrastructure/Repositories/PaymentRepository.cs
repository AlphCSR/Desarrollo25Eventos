using Microsoft.EntityFrameworkCore;
using PaymentsMS.Domain.Entities;
using PaymentsMS.Domain.Interfaces;
using PaymentsMS.Infrastructure.Persistence;
using System.Threading;
using System.Threading.Tasks;

namespace PaymentsMS.Infrastructure.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly PaymentsDbContext _context;

        public PaymentRepository(PaymentsDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Payment payment, CancellationToken cancellationToken)
        {
            await _context.Payments.AddAsync(payment, cancellationToken);
        }

        public async Task<Payment?> GetByPaymentIntentIdAsync(string paymentIntentId, CancellationToken cancellationToken)
        {
            return await _context.Payments
                .FirstOrDefaultAsync(p => p.StripePaymentIntentId == paymentIntentId, cancellationToken);
        }

        public async Task<Payment?> GetByBookingIdAsync(Guid bookingId)
        {
            return await _context.Payments
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync(p => p.BookingId == bookingId);
        }

        public async Task<System.Collections.Generic.IEnumerable<Payment>> GetPendingPaymentsOlderThanAsync(System.DateTime date, CancellationToken cancellationToken)
        {
            return await _context.Payments
                .Where(p => p.Status == "Pending" && p.CreatedAt < date && p.StripePaymentIntentId != null)
                .ToListAsync(cancellationToken);
        }

        public async Task UpdateAsync(Payment payment, CancellationToken cancellationToken)
        {
            _context.Payments.Update(payment);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
