using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MarketingMS.Domain.Entities;

namespace MarketingMS.Domain.Interfaces
{
    public interface ICouponRepository
    {
        Task<Coupon> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Coupon> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
        Task<IEnumerable<Coupon>> GetAllAsync(CancellationToken cancellationToken = default);
        Task AddAsync(Coupon coupon, CancellationToken cancellationToken = default);
        Task UpdateAsync(Coupon coupon, CancellationToken cancellationToken = default);
        Task DeleteAsync(Coupon coupon, CancellationToken cancellationToken = default);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
