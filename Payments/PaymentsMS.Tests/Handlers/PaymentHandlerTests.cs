using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MassTransit;
using Moq;
using PaymentsMS.Application.Commands.CapturePayment;
using PaymentsMS.Application.Commands.CreatePayment;
using PaymentsMS.Application.DTOs;
using PaymentsMS.Domain.Entities;
using PaymentsMS.Domain.Interfaces;
using PaymentsMS.Domain.Models;
using Xunit;

namespace PaymentsMS.Tests.Handlers
{
    public class PaymentHandlerTests
    {
        private readonly Mock<IPaymentRepository> _repositoryMock;
        private readonly Mock<IPaymentGateway> _gatewayMock;
        private readonly Mock<IPublishEndpoint> _publishEndpointMock;

        public PaymentHandlerTests()
        {
            _repositoryMock = new Mock<IPaymentRepository>();
            _gatewayMock = new Mock<IPaymentGateway>();
            _publishEndpointMock = new Mock<IPublishEndpoint>();
        }

        [Fact]
        public async Task Handle_CreatePaymentIntent_Tests()
        {
            var handler = new CreatePaymentIntentCommandHandler(_gatewayMock.Object, _repositoryMock.Object);
            
            var bookingId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var dto = new CreatePaymentDto(bookingId, userId, 100, "USD", "test@example.com");
            
            _gatewayMock.Setup(x => x.CreatePaymentIntentAsync(100, "USD", bookingId.ToString(), userId))
                .ReturnsAsync(new GatewayPaymentIntent("pi_123", "secret_123"));

            // --- Success ---
            var result = await handler.Handle(new CreatePaymentIntentCommand(dto), CancellationToken.None);

            result.Should().NotBeNull();
            result.StripePaymentIntentId.Should().Be("pi_123");
            _repositoryMock.Verify(x => x.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_CapturePayment_Tests()
        {
            var handler = new CapturePaymentCommandHandler(_repositoryMock.Object, _publishEndpointMock.Object);
            var piId = "pi_123";
            var payment = new Payment(Guid.NewGuid(), Guid.NewGuid(), 100, "USD", "test@example.com");
            payment.SetStripePaymentIntentId(piId);

            // --- Success ---
            _repositoryMock.Setup(x => x.GetByPaymentIntentIdAsync(piId, It.IsAny<CancellationToken>())).ReturnsAsync(payment);
            
            var result = await handler.Handle(new CapturePaymentCommand(piId), CancellationToken.None);
            
            result.Should().BeTrue();
            payment.Status.Should().Be("Succeeded");
            _publishEndpointMock.Verify(x => x.Publish(It.IsAny<BookingMS.Shared.Events.PaymentCapturedEvent>(), It.IsAny<CancellationToken>()), Times.Once);

            // --- Not Found ---
            _repositoryMock.Setup(x => x.GetByPaymentIntentIdAsync("pi_unknown", It.IsAny<CancellationToken>())).ReturnsAsync((Payment?)null);
            var result2 = await handler.Handle(new CapturePaymentCommand("pi_unknown"), CancellationToken.None);
            result2.Should().BeFalse();
        }
    }
}
