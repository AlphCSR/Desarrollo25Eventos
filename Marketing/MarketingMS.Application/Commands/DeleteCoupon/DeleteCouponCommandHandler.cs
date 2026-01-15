using MediatR;
using MarketingMS.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace MarketingMS.Application.Commands.DeleteCoupon
{
    public class DeleteCouponCommandHandler : IRequestHandler<DeleteCouponCommand>
    {
        private readonly ICouponRepository _repository;

        public DeleteCouponCommandHandler(ICouponRepository repository)
        {
            _repository = repository;
        }

        public async Task Handle(DeleteCouponCommand request, CancellationToken cancellationToken)
        {
            var coupon = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (coupon != null)
            {
                await _repository.DeleteAsync(coupon, cancellationToken);
                await _repository.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
