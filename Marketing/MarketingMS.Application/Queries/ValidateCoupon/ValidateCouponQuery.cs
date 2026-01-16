using MediatR;
using MarketingMS.Application.DTOs;

namespace MarketingMS.Application.Queries.ValidateCoupon
{
    public class ValidateCouponQuery : IRequest<CouponDto?>
    {
        public string Code { get; set; }
        public decimal Amount { get; set; }

        public ValidateCouponQuery(string code, decimal amount)
        {
            Code = code;
            Amount = amount;
        }
    }
}
