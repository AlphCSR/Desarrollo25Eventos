using System;
using MarketingMS.Domain.ValueObjects;
using MarketingMS.Domain.Exceptions;

namespace MarketingMS.Domain.Entities
{
    public enum DiscountType
    {
        Percentage,
        FixedAmount
    }

    public class Coupon
    {
        public Guid Id { get; private set; }
        public CouponCode Code { get; private set; }
        public DiscountType Type { get; private set; }
        public decimal Value { get; private set; }
        public DateTime? ExpiryDate { get; private set; }
        public int? UsageLimit { get; private set; }
        public int CurrentUsage { get; private set; }
        public bool IsActive { get; private set; }
        public decimal? MinimumAmount { get; private set; }
 
        private Coupon() 
        { 
            Code = null!;
        }

        public Coupon(string code, DiscountType type, decimal value, DateTime? expiryDate = null, int? usageLimit = null, decimal? minimumAmount = null)
        {
            if (value <= 0) throw new InvalidMarketingDataException("El valor debe ser mayor a cero.");

            Id = Guid.NewGuid();
            Code = CouponCode.Create(code);
            Type = type;
            Value = value;
            ExpiryDate = expiryDate;
            UsageLimit = usageLimit;
            MinimumAmount = minimumAmount;
            CurrentUsage = 0;
            IsActive = true;
        }

        public bool IsValid(decimal amount)
        {
            if (!IsActive) return false;
            if (ExpiryDate.HasValue && ExpiryDate.Value < DateTime.UtcNow) return false;
            if (UsageLimit.HasValue && CurrentUsage >= UsageLimit.Value) return false;
            if (MinimumAmount.HasValue && amount < MinimumAmount.Value) return false;
            
            return true;
        }

        public void IncrementUsage()
        {
            CurrentUsage++;
        }

        public void Deactivate()
        {
            IsActive = false;
        }
    }
}
