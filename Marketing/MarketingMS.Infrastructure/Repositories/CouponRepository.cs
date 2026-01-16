using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MarketingMS.Domain.Entities;
using MarketingMS.Domain.Interfaces;
using MarketingMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MarketingMS.Infrastructure.Repositories
{
    public class CouponRepository : ICouponRepository
    {
        private readonly MarketingDbContext _context;

        public CouponRepository(MarketingDbContext context)
        {
            _context = context;
        }

        public async Task<Coupon> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Coupons.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public async Task<Coupon> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            return await _context.Coupons.FirstOrDefaultAsync(c => c.Code == code.ToUpper(), cancellationToken);
        }

        public async Task<IEnumerable<Coupon>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Coupons.ToListAsync(cancellationToken);
        }

        public async Task AddAsync(Coupon coupon, CancellationToken cancellationToken = default)
        {
            await _context.Coupons.AddAsync(coupon, cancellationToken);
        }

        public async Task UpdateAsync(Coupon coupon, CancellationToken cancellationToken = default)
        {
            _context.Coupons.Update(coupon);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Coupon coupon, CancellationToken cancellationToken = default)
        {
            _context.Coupons.Remove(coupon);
            await Task.CompletedTask;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
