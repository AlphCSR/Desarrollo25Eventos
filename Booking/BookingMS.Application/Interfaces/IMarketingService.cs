using System;
using System.Threading;
using System.Threading.Tasks;

namespace BookingMS.Application.Interfaces
{
    public interface IMarketingService
    {
        Task<CouponValidationResult?> ValidateCouponAsync(string code, decimal amount, CancellationToken cancellationToken = default);
    }

    public class CouponValidationResult
    {
        public string Code { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public decimal DiscountAmount { get; set; }
    }
}
