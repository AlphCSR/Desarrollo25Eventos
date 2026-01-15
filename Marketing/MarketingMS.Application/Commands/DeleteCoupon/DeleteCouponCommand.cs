using MediatR;
using System;

namespace MarketingMS.Application.Commands.DeleteCoupon
{
    public record DeleteCouponCommand(Guid Id) : IRequest;
}
