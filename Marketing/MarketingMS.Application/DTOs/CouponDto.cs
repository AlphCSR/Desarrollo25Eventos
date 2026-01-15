using MarketingMS.Domain.Entities;
using System;

namespace MarketingMS.Application.DTOs
{
    public class CouponDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Type { get; set; }
        public decimal Value { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int? UsageLimit { get; set; }
        public int CurrentUsage { get; set; }
        public bool IsActive { get; set; }
        public decimal? MinimumAmount { get; set; }

        public static CouponDto FromEntity(Coupon coupon)
        {
            return new CouponDto
            {
                Id = coupon.Id,
                Code = coupon.Code,
                Type = coupon.Type.ToString(),
                Value = coupon.Value,
                ExpiryDate = coupon.ExpiryDate,
                UsageLimit = coupon.UsageLimit,
                CurrentUsage = coupon.CurrentUsage,
                IsActive = coupon.IsActive,
                MinimumAmount = coupon.MinimumAmount
            };
        }
    }
}
