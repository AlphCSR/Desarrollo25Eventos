using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using FluentAssertions;
using PaymentsMS.Application.Commands.CreatePayment;
using PaymentsMS.Application.DTOs;
using PaymentsMS.Domain.Interfaces;
using PaymentsMS.Domain.Models;
using PaymentsMS.Domain.Entities;

namespace PaymentsMS.Tests.Application
{
    public class CreatePaymentIntentCommandHandlerTests
    {
        private readonly Mock<IPaymentGateway> _gatewayMock;
        private readonly Mock<IPaymentRepository> _repoMock;
        private readonly CreatePaymentIntentCommandHandler _handler;

        public CreatePaymentIntentCommandHandlerTests()
        {
            _gatewayMock = new Mock<IPaymentGateway>();
            _repoMock = new Mock<IPaymentRepository>();
            _handler = new CreatePaymentIntentCommandHandler(_gatewayMock.Object, _repoMock.Object);
        }

        [Fact]
        public async Task Handle_CreatePaymentIntent_Success()
        {
            var dto = new CreatePaymentDto(Guid.NewGuid(), Guid.NewGuid(), 100m, "USD", "test@test.com");
            var command = new CreatePaymentIntentCommand(dto);
            
            var gatewayResponse = new GatewayPaymentIntent("pi_12345", "secret_12345");

            _gatewayMock.Setup(g => g.CreatePaymentIntentAsync(dto.Amount, dto.Currency, dto.BookingId.ToString(), dto.UserId))
                .ReturnsAsync(gatewayResponse);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.ClientSecret.Should().Be("secret_12345");
            result.StripePaymentIntentId.Should().Be("pi_12345");

            _repoMock.Verify(r => r.AddAsync(It.Is<Payment>(p => 
                p.BookingId == dto.BookingId && 
                p.StripePaymentIntentId == "pi_12345"), It.IsAny<CancellationToken>()), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
