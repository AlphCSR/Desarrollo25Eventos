using MediatR;
using MarketingMS.Domain.Entities;
using MarketingMS.Domain.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MarketingMS.Application.Commands.CreateCoupon
{
    public class CreateCouponCommandHandler : IRequestHandler<CreateCouponCommand, Guid>
    {
        private readonly ICouponRepository _repository;

        public CreateCouponCommandHandler(ICouponRepository repository)
        {
            _repository = repository;
        }

        public async Task<Guid> Handle(CreateCouponCommand request, CancellationToken cancellationToken)
        {
            var existing = await _repository.GetByCodeAsync(request.Code, cancellationToken);
            if (existing != null) throw new Exception("El código de cupón ya existe");

            var coupon = new Coupon(request.Code, request.Type, request.Value, request.ExpiryDate, request.UsageLimit, request.MinimumAmount);
            
            await _repository.AddAsync(coupon, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            return coupon.Id;
        }
    }
}
