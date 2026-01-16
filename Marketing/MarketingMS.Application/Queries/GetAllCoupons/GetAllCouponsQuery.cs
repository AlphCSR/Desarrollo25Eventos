using MediatR;
using MarketingMS.Application.DTOs;
using System.Collections.Generic;

namespace MarketingMS.Application.Queries.GetAllCoupons
{
    public record GetAllCouponsQuery() : IRequest<List<CouponDto>>;
}
