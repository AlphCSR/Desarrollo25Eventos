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
using Xunit;

namespace SeatingMS.Tests.Handlers
{
    public class SeatHandlerTests
    {
        private readonly Mock<IEventSeatRepository> _repositoryMock;
        private readonly Mock<IPublishEndpoint> _publishEndpointMock;

        public SeatHandlerTests()
        {
            _repositoryMock = new Mock<IEventSeatRepository>();
            _publishEndpointMock = new Mock<IPublishEndpoint>();
        }

        // =================================================================================================
        // 1. LOCK SEAT
        // =================================================================================================

        [Theory]
        [MemberData(nameof(GetLockSeatSuccessScenarios))]
        public async Task Handle_LockSeat_Success(string scenarioName, Guid seatId, Guid userId, EventSeat seat)
        {
            // Arrange
            _repositoryMock.Setup(x => x.GetByIdAsync(seatId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(seat);

            var handler = new LockSeatCommandHandler(_repositoryMock.Object);
            var command = new LockSeatCommand(seatId, userId);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            _repositoryMock.Verify(x => x.UpdateAsync(seat, It.IsAny<CancellationToken>()), Times.Once);
            _repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            seat.Status.Should().Be(Shared.Enum.SeatStatus.Locked);
        }

        [Theory]
        [MemberData(nameof(GetLockSeatFailureScenarios))]
        public async Task Handle_LockSeat_Failure(string scenarioName, Guid seatId, Guid userId, EventSeat? seatInDb, Type expectedExceptionType)
        {
            // Arrange
            _repositoryMock.Setup(x => x.GetByIdAsync(seatId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(seatInDb);

            var handler = new LockSeatCommandHandler(_repositoryMock.Object);
            var command = new LockSeatCommand(seatId, userId);

            // Act
            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .Where(e => e.GetType() == expectedExceptionType);
        }

        public static IEnumerable<object?[]> GetLockSeatSuccessScenarios()
        {
            var seatId = Guid.NewGuid();
            var seat = new EventSeat(Guid.NewGuid(), Guid.NewGuid(), "A", 1);
            typeof(EventSeat).GetProperty("Id")?.SetValue(seat, seatId);

            yield return new object?[] { "Success_AvailableSeat", seatId, Guid.NewGuid(), seat };
        }

        public static IEnumerable<object?[]> GetLockSeatFailureScenarios()
        {
            var seatId = Guid.NewGuid();
            
            // Case 1: Not Found
            yield return new object?[] { "Fail_NotFound", seatId, Guid.NewGuid(), null, typeof(SeatNotFoundException) };

            // Case 2: Already Locked
            var lockedSeat = new EventSeat(Guid.NewGuid(), Guid.NewGuid(), "B", 2);
            typeof(EventSeat).GetProperty("Id")?.SetValue(lockedSeat, seatId);
            lockedSeat.Lock(Guid.NewGuid()); // Lock by someone else

            yield return new object?[] { "Fail_AlreadyLocked", seatId, Guid.NewGuid(), lockedSeat, typeof(SeatNotAvailableException) };
        }

        // =================================================================================================
        // 2. UNLOCK SEAT
        // =================================================================================================

        [Theory]
        [MemberData(nameof(GetUnlockSeatSuccessScenarios))]
        public async Task Handle_UnlockSeat_Success(string scenarioName, Guid seatId, Guid userId, EventSeat seat)
        {
            // Arrange
            _repositoryMock.Setup(x => x.GetByIdAsync(seatId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(seat);

            var handler = new UnlockSeatCommandHandler(_repositoryMock.Object, _publishEndpointMock.Object);
            var command = new UnlockSeatCommand(seatId, userId);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            _repositoryMock.Verify(x => x.UpdateAsync(seat, It.IsAny<CancellationToken>()), Times.Once);
            _repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _publishEndpointMock.Verify(x => x.Publish(It.IsAny<Shared.Events.SeatUnlockedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            seat.Status.Should().Be(Shared.Enum.SeatStatus.Available);
        }

        [Theory]
        [MemberData(nameof(GetUnlockSeatFailureScenarios))]
        public async Task Handle_UnlockSeat_Failure(string scenarioName, Guid seatId, Guid userId, EventSeat? seatInDb, Type expectedExceptionType, string expectedMessage)
        {
            // Arrange
            _repositoryMock.Setup(x => x.GetByIdAsync(seatId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(seatInDb);

            var handler = new UnlockSeatCommandHandler(_repositoryMock.Object, _publishEndpointMock.Object);
            var command = new UnlockSeatCommand(seatId, userId);

            // Act
            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .Where(e => e.GetType() == expectedExceptionType)
                .WithMessage($"*{expectedMessage}*");
        }

        public static IEnumerable<object?[]> GetUnlockSeatSuccessScenarios()
        {
            var seatId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var seat = new EventSeat(Guid.NewGuid(), Guid.NewGuid(), "C", 3);
            typeof(EventSeat).GetProperty("Id")?.SetValue(seat, seatId);
            seat.Lock(userId); // Lock by me

            yield return new object?[] { "Success_UnlockMySeat", seatId, userId, seat };
        }

        public static IEnumerable<object?[]> GetUnlockSeatFailureScenarios()
        {
            var seatId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            // Case 1: Not Found
            yield return new object?[] { "Fail_NotFound", seatId, userId, null, typeof(Exception), "Asiento no encontrado" };

            // Case 2: Locked by someone else
            var otherUser = Guid.NewGuid();
            var lockedSeat = new EventSeat(Guid.NewGuid(), Guid.NewGuid(), "D", 4);
            typeof(EventSeat).GetProperty("Id")?.SetValue(lockedSeat, seatId);
            lockedSeat.Lock(otherUser);

            yield return new object?[] { "Fail_NotMyLock", seatId, userId, lockedSeat, typeof(InvalidOperationException), "El asiento no está bloqueado por este usuario" };
        }
    }
}
