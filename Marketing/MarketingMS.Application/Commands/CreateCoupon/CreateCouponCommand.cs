using MediatR;
using MarketingMS.Domain.Entities;
using System;

namespace MarketingMS.Application.Commands.CreateCoupon
{
    public class CreateCouponCommand : IRequest<Guid>
    {
        public string Code { get; set; }
        public DiscountType Type { get; set; }
        public decimal Value { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int? UsageLimit { get; set; }
        public decimal? MinimumAmount { get; set; }

        public CreateCouponCommand(string code, DiscountType type, decimal value, DateTime? expiryDate = null, int? usageLimit = null, decimal? minimumAmount = null)
        {
            Code = code;
            Type = type;
            Value = value;
            ExpiryDate = expiryDate;
            UsageLimit = usageLimit;
            MinimumAmount = minimumAmount;
        }
    }
}
