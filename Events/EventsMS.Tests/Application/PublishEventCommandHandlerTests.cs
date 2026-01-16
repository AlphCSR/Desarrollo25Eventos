using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Moq;
using EventsMS.Application.Commands.PublishEvent;
using EventsMS.Domain.Interfaces;
using EventsMS.Shared.Events;
using MassTransit;
using EventsMS.Domain.Entities;
using EventsMS.Shared.Enums;
using System.Collections.Generic;

namespace EventsMS.Tests.Application
{
    public class PublishEventCommandHandlerTests
    {
        private readonly Mock<IEventRepository> _repoMock;
        private readonly Mock<IPublishEndpoint> _publishMock;
        private readonly PublishEventCommandHandler _handler;

        public PublishEventCommandHandlerTests()
        {
            _repoMock = new Mock<IEventRepository>();
            _publishMock = new Mock<IPublishEndpoint>();
            _handler = new PublishEventCommandHandler(_repoMock.Object, _publishMock.Object);
        }

        [Fact]
        public async Task Handle_PublishEvent_ShouldWork()
        {
            var eventEntity = new EventsMS.Domain.Entities.Event(
                Guid.NewGuid(),
                "Title",
                "Desc",
                DateTime.UtcNow.AddDays(5),
                DateTime.UtcNow.AddDays(6),
                "Venue",
                new List<string> { "Music" },
                EventType.Physical,
                "http://url.com"
            );
            
            eventEntity.AddSection("General", 50, 100, false);

            _repoMock.Setup(r => r.GetByIdAsync(eventEntity.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(eventEntity);

            var result = await _handler.Handle(new PublishEventCommand(eventEntity.Id), CancellationToken.None);

            result.Should().BeTrue();
            eventEntity.Status.Should().Be(EventStatus.Published);
            _repoMock.Verify(r => r.UpdateAsync(eventEntity, It.IsAny<CancellationToken>()), Times.Once);
            _publishMock.Verify(p => p.Publish(It.IsAny<EventStatusChangedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
