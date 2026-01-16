using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MarketingMS.Application.Commands.CreateCoupon;
using MarketingMS.Application.Commands.DeleteCoupon;
using MarketingMS.Application.Queries.GetAllCoupons;
using MarketingMS.Application.Queries.GetRecommendations;
using MarketingMS.Application.Interfaces;
using MarketingMS.Domain.Entities;
using MarketingMS.Domain.Interfaces;
using Moq;
using FluentAssertions;
using Xunit;

namespace MarketingMS.Tests.Handlers
{
    public class MarketingHandlerTests
    {
        private readonly Mock<ICouponRepository> _couponRepoMock;
        private readonly Mock<IUserInterestRepository> _interestRepoMock;
        private readonly Mock<IEventsService> _eventsServiceMock;

        public MarketingHandlerTests()
        {
            _couponRepoMock = new Mock<ICouponRepository>();
            _interestRepoMock = new Mock<IUserInterestRepository>();
            _eventsServiceMock = new Mock<IEventsService>();
        }

        [Fact]
        public async Task Handle_CreateCoupon_ShouldSuccess()
        {
            var handler = new CreateCouponCommandHandler(_couponRepoMock.Object);
            var command = new CreateCouponCommand(
                "NEW2026", 
                DiscountType.Percentage, 
                10, 
                DateTime.UtcNow.AddDays(1) 
            );

            _couponRepoMock.Setup(x => x.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Coupon?)null);

            var result = await handler.Handle(command, CancellationToken.None);

            result.Should().NotBeEmpty();
            _couponRepoMock.Verify(x => x.AddAsync(It.IsAny<Coupon>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_DeleteCoupon_ShouldSuccess()
        {
            var handler = new DeleteCouponCommandHandler(_couponRepoMock.Object);
            var couponId = Guid.NewGuid();
            var coupon = new Coupon("DEL", DiscountType.FixedAmount, 10, DateTime.UtcNow.AddDays(1));

            _couponRepoMock.Setup(x => x.GetByIdAsync(couponId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(coupon);

            await handler.Handle(new DeleteCouponCommand(couponId), CancellationToken.None);

            _couponRepoMock.Verify(x => x.DeleteAsync(coupon, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_GetAllCoupons_ShouldReturnList()
        {
            var handler = new GetAllCouponsQueryHandler(_couponRepoMock.Object);
            _couponRepoMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Coupon> { new Coupon("C1", DiscountType.Percentage, 10, DateTime.UtcNow) });

            var result = await handler.Handle(new GetAllCouponsQuery(), CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_GetRecommendations_ShouldReturnEvents()
        {
            var userId = Guid.NewGuid();
            var handler = new GetRecommendationsQueryHandler(_interestRepoMock.Object, _eventsServiceMock.Object);
            
            _interestRepoMock.Setup(x => x.GetTopCategoriesAsync(userId, It.IsAny<int>()))
                .ReturnsAsync(new List<string> { "Rock" });

            _eventsServiceMock.Setup(x => x.GetEventsAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(new List<EventDetailsDto> { new EventDetailsDto { Title = "Rock Fest" } });

            var result = await handler.Handle(new GetRecommendationsQuery(userId), CancellationToken.None);

            result.Should().HaveCount(1);
            result.Should().Contain(e => e.Title == "Rock Fest");
        }
    }
}
