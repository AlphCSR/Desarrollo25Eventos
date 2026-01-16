using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using FluentAssertions;
using PaymentsMS.Application.Commands.CapturePayment;
using PaymentsMS.Domain.Interfaces;
using PaymentsMS.Domain.Entities;
using MassTransit;
using BookingMS.Shared.Events;

namespace PaymentsMS.Tests.Application
{
    public class CapturePaymentCommandHandlerTests
    {
        private readonly Mock<IPaymentRepository> _repoMock;
        private readonly Mock<IPublishEndpoint> _publishMock;
        private readonly CapturePaymentCommandHandler _handler;

        public CapturePaymentCommandHandlerTests()
        {
            _repoMock = new Mock<IPaymentRepository>();
            _publishMock = new Mock<IPublishEndpoint>();
            _handler = new CapturePaymentCommandHandler(_repoMock.Object, _publishMock.Object);
        }

        [Fact]
        public async Task Handle_CapturePayment_Success()
        {
            var paymentIntentId = "pi_12345";
            var bookingId = Guid.NewGuid();
            var payment = new Payment(bookingId, Guid.NewGuid(), 100m, "USD", "test@test.com");
            payment.SetStripePaymentIntentId(paymentIntentId);

            _repoMock.Setup(r => r.GetByPaymentIntentIdAsync(paymentIntentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(payment);

            var command = new CapturePaymentCommand(paymentIntentId);
            var result = await _handler.Handle(command, CancellationToken.None);

            result.Should().BeTrue();
            payment.Status.Should().Be("Succeeded");
            _repoMock.Verify(r => r.UpdateAsync(payment, It.IsAny<CancellationToken>()), Times.Once);
            _publishMock.Verify(p => p.Publish(It.IsAny<PaymentCapturedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_CapturePayment_NotFound_ShouldReturnFalse()
        {
            _repoMock.Setup(r => r.GetByPaymentIntentIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Payment?)null);

            var command = new CapturePaymentCommand("unknown");
            var result = await _handler.Handle(command, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
