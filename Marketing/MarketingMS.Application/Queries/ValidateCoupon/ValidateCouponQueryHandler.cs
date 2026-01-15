using MediatR;
using MarketingMS.Application.DTOs;
using MarketingMS.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace MarketingMS.Application.Queries.ValidateCoupon
{
    public class ValidateCouponQueryHandler : IRequestHandler<ValidateCouponQuery, CouponDto?>
    {
        private readonly ICouponRepository _repository;

        public ValidateCouponQueryHandler(ICouponRepository repository)
        {
            _repository = repository;
        }

        public async Task<CouponDto?> Handle(ValidateCouponQuery request, CancellationToken cancellationToken)
        {
            var cleanCode = request.Code?.Trim().ToUpper();
            if (string.IsNullOrEmpty(cleanCode)) return null;

            var coupon = await _repository.GetByCodeAsync(cleanCode, cancellationToken);
            
            if (coupon == null || !coupon.IsValid(request.Amount))
            {
                return null;
            }

            return CouponDto.FromEntity(coupon);
        }
    }
}
