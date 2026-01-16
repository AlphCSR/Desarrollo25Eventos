using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Moq;
using EventsMS.Application.Queries.GetEvents;
using EventsMS.Domain.Interfaces;
using EventsMS.Application.DTOs;
using EventsMS.Domain.Entities;
using EventsMS.Shared.Enums;

namespace EventsMS.Tests.Application
{
    public class GetEventsQueryHandlerTests
    {
        private readonly Mock<IEventRepository> _repoMock;
        private readonly GetEventsQueryHandler _handler;

        public GetEventsQueryHandlerTests()
        {
            _repoMock = new Mock<IEventRepository>();
            _handler = new GetEventsQueryHandler(_repoMock.Object);
        }

        [Fact]
        public async Task Handle_GetEvents_ShouldReturnPagedResult()
        {
            var eventEntity = new EventsMS.Domain.Entities.Event(
                Guid.NewGuid(),
                "Title",
                "Desc",
                DateTime.UtcNow,
                DateTime.UtcNow.AddHours(2),
                "Venue",
                new List<string> { "Cat1" },
                EventType.Physical,
                null
            );

            var items = new List<EventsMS.Domain.Entities.Event> { eventEntity };
            _repoMock.Setup(r => r.GetPagedAsync(
                It.IsAny<int>(), It.IsAny<int>(), 
                It.IsAny<string?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(),
                It.IsAny<decimal?>(), It.IsAny<decimal?>(), It.IsAny<int?>(),
                It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((items, 1));

            var query = new GetEventsQuery { Page = 1, PageSize = 10 };
            var result = await _handler.Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result.TotalCount.Should().Be(1);
            result.Items.Should().HaveCount(1);
        }
    }
}
