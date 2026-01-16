using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MassTransit;
using Moq;
using SeatingMS.Application.Commands.LockSeat;
using SeatingMS.Application.Commands.UnlockSeat;
using SeatingMS.Domain.Entities;
using SeatingMS.Domain.Exceptions;
using SeatingMS.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using SeatingMS.Shared.Enum;
using Xunit;

namespace SeatingMS.Tests.Handlers
{
    public class SeatHandlerTests
    {
        private readonly Mock<IEventSeatRepository> _repositoryMock;
        private readonly Mock<IPublishEndpoint> _publishEndpointMock;
        private readonly Mock<ILogger<UnlockSeatCommandHandler>> _loggerMock;

        public SeatHandlerTests()
        {
            _repositoryMock = new Mock<IEventSeatRepository>();
            _publishEndpointMock = new Mock<IPublishEndpoint>();
            _loggerMock = new Mock<ILogger<UnlockSeatCommandHandler>>();
        }

        [Fact]
        public async Task Handle_LockSeat_Tests()
        {
            var handler = new LockSeatCommandHandler(_repositoryMock.Object, _publishEndpointMock.Object);
            var seatId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var seat = new EventSeat(Guid.NewGuid(), Guid.NewGuid(), "A", 1);
            typeof(EventSeat).GetProperty("Id")?.SetValue(seat, seatId);


            _repositoryMock.Setup(x => x.GetByIdAsync(seatId, It.IsAny<CancellationToken>())).ReturnsAsync(seat);
            
            var result = await handler.Handle(new LockSeatCommand(seatId, userId), CancellationToken.None);
            
            result.Should().BeTrue();
            seat.Status.Should().Be(SeatStatus.Locked);
            _repositoryMock.Verify(x => x.UpdateAsync(seat, It.IsAny<CancellationToken>()), Times.Once);

            Func<Task> act1 = async () => await handler.Handle(new LockSeatCommand(seatId, Guid.NewGuid()), CancellationToken.None);
            await act1.Should().ThrowAsync<SeatNotAvailableException>();

            var unknownId = Guid.NewGuid();
            _repositoryMock.Setup(x => x.GetByIdAsync(unknownId, It.IsAny<CancellationToken>())).ReturnsAsync((EventSeat?)null);
            
            Func<Task> act2 = async () => await handler.Handle(new LockSeatCommand(unknownId, userId), CancellationToken.None);
            await act2.Should().ThrowAsync<SeatNotFoundException>();
        }

        [Fact]
        public async Task Handle_UnlockSeat_Tests()
        {
            var handler = new UnlockSeatCommandHandler(_repositoryMock.Object, _publishEndpointMock.Object, _loggerMock.Object);
            var seatId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var seat = new EventSeat(Guid.NewGuid(), Guid.NewGuid(), "B", 2);
            typeof(EventSeat).GetProperty("Id")?.SetValue(seat, seatId);
            seat.Lock(userId);

            _repositoryMock.Setup(x => x.GetByIdAsync(seatId, It.IsAny<CancellationToken>())).ReturnsAsync(seat);
            
            var result = await handler.Handle(new UnlockSeatCommand(seatId, userId), CancellationToken.None);
            
            result.Should().BeTrue();
            seat.Status.Should().Be(SeatStatus.Available);
            _repositoryMock.Verify(x => x.UpdateAsync(seat, It.IsAny<CancellationToken>()), Times.Once);

            
            var unknownId = Guid.NewGuid();
            _repositoryMock.Setup(x => x.GetByIdAsync(unknownId, It.IsAny<CancellationToken>())).ReturnsAsync((EventSeat?)null);
            
            Func<Task> act = async () => await handler.Handle(new UnlockSeatCommand(unknownId, userId), CancellationToken.None);
            await act.Should().ThrowAsync<Exception>().WithMessage("Asiento no encontrado");
        }
    }
}
