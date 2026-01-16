using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Moq;
using EventsMS.Application.Queries.GetEventById;
using EventsMS.Domain.Interfaces;
using EventsMS.Domain.Entities;
using EventsMS.Shared.Enums;
using EventsMS.Domain.Exceptions;
using System.Collections.Generic;

namespace EventsMS.Tests.Application
{
    public class GetEventByIdQueryHandlerTests
    {
        private readonly Mock<IEventRepository> _repoMock;
        private readonly GetEventByIdQueryHandler _handler;

        public GetEventByIdQueryHandlerTests()
        {
            _repoMock = new Mock<IEventRepository>();
            _handler = new GetEventByIdQueryHandler(_repoMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnEvent_WhenExists()
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

            var result = await _handler.Handle(new GetEventByIdQuery(eventEntity.Id), CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(eventEntity.Id);
            result.Title.Should().Be("Title");
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenNotFound()
        {
            var eventId = Guid.NewGuid();
            _repoMock.Setup(r => r.GetByIdAsync(eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((EventsMS.Domain.Entities.Event)null);

            Func<Task> act = async () => await _handler.Handle(new GetEventByIdQuery(eventId), CancellationToken.None);

            await act.Should().ThrowAsync<EventNotFoundException>();
        }
    }
}
