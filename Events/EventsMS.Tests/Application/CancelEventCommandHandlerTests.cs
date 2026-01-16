using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Moq;
using EventsMS.Application.Commands.CancelEvent;
using EventsMS.Domain.Interfaces;
using EventsMS.Shared.Events;
using MassTransit;
using EventsMS.Shared.Enums;

namespace EventsMS.Tests.Application
{
    public class CancelEventCommandHandlerTests
    {
        private readonly Mock<IEventRepository> _repoMock;
        private readonly Mock<IPublishEndpoint> _publishMock;
        private readonly CancelEventCommandHandler _handler;

        public CancelEventCommandHandlerTests()
        {
            _repoMock = new Mock<IEventRepository>();
            _publishMock = new Mock<IPublishEndpoint>();
            _handler = new CancelEventCommandHandler(_repoMock.Object, _publishMock.Object);
        }

        [Fact]
        public async Task Handle_CancelEvent_Success()
        {
            var userId = Guid.NewGuid();
            var eventEntity = new EventsMS.Domain.Entities.Event(
                userId,
                "Title",
                "Desc",
                DateTime.UtcNow,
                DateTime.UtcNow.AddHours(2),
                "Venue",
                new List<string> { "Cat1" },
                EventType.Physical,
                null
            );

            _repoMock.Setup(r => r.GetByIdAsync(eventEntity.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(eventEntity);

            var command = new CancelEventCommand(eventEntity.Id, userId, false, "Bad weather");
            var result = await _handler.Handle(command, CancellationToken.None);

            result.Should().BeTrue();
            eventEntity.Status.Should().Be(EventStatus.Cancelled);
            _publishMock.Verify(p => p.Publish(It.IsAny<EventCancelledEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_CancelEvent_Unauthorized_ShouldThrow()
        {
            var userId = Guid.NewGuid();
            var eventEntity = new EventsMS.Domain.Entities.Event(
                userId,
                "Title",
                "Desc",
                DateTime.UtcNow,
                DateTime.UtcNow.AddHours(2),
                "Venue",
                new List<string> { "Cat1" },
                EventType.Physical,
                null
            );

            _repoMock.Setup(r => r.GetByIdAsync(eventEntity.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(eventEntity);

            var command = new CancelEventCommand(eventEntity.Id, Guid.NewGuid(), false, "Malicious cancel");

            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }
    }
}
