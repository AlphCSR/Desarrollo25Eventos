using MediatR;
using MarketingMS.Application.DTOs;
using MarketingMS.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MarketingMS.Application.Queries.GetAllCoupons
{
    public class GetAllCouponsQueryHandler : IRequestHandler<GetAllCouponsQuery, List<CouponDto>>
    {
        private readonly ICouponRepository _repository;

        public GetAllCouponsQueryHandler(ICouponRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<CouponDto>> Handle(GetAllCouponsQuery request, CancellationToken cancellationToken)
        {
            var coupons = await _repository.GetAllAsync(cancellationToken);
            return coupons.Select(CouponDto.FromEntity).ToList();
        }
    }
}
