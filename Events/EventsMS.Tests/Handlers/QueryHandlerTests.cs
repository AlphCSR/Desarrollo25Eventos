using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Xunit;
using EventsMS.Application.Queries.GetEvents;
using EventsMS.Application.Queries.GetEventById;
using EventsMS.Application.Queries.GetEventSections;
using EventsMS.Domain.Interfaces;
using EventsMS.Domain.Entities;
using EventEntity = EventsMS.Domain.Entities.Event;

namespace EventsMS.Tests.Handlers
{
    public class QueryHandlerTests
    {
        private readonly Mock<IEventRepository> _repositoryMock;

        public QueryHandlerTests()
        {
            _repositoryMock = new Mock<IEventRepository>();
        }

        [Fact]
        public async Task Handle_GetEvents_ShouldReturnList()
        {
            // Arrange
            var events = new List<EventEntity>
            {
                new EventEntity("E1", "D1", DateTime.UtcNow.AddDays(1), "V1", "C1"),
                new EventEntity("E2", "D2", DateTime.UtcNow.AddDays(1), "V2", "C2")
            };
            _repositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(events);

            var handler = new GetEventsQueryHandler(_repositoryMock.Object);

            // Act
            var result = await handler.Handle(new GetEventsQuery(), CancellationToken.None);

            // Assert
            result.Should().HaveCount(2);
            result[0].Title.Should().Be("E1");
        }

        [Fact]
        public async Task Handle_GetEventById_ShouldReturnDto_WhenFound()
        {
            // Arrange
            var evt = new EventEntity("E1", "D1", DateTime.UtcNow.AddDays(1), "V1", "C1");
            _repositoryMock.Setup(x => x.GetByIdAsync(evt.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(evt);

            var handler = new GetEventByIdQueryHandler(_repositoryMock.Object);

            // Act
            var result = await handler.Handle(new GetEventByIdQuery(evt.Id), CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.Title.Should().Be("E1");
        }

        [Fact]
        public async Task Handle_GetEventById_ShouldThrowException_WhenNotFound()
        {
            // Arrange
            _repositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((EventEntity?)null);

            var handler = new GetEventByIdQueryHandler(_repositoryMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<EventsMS.Domain.Exceptions.EventNotFoundException>(() => 
                handler.Handle(new GetEventByIdQuery(Guid.NewGuid()), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_GetEventSections_ShouldReturnSections()
        {
            // Arrange
            var evt = new EventEntity("E1", "D1", DateTime.UtcNow.AddDays(1), "V1", "C1");
            evt.AddSection("S1", 100, 50, true);
            
            _repositoryMock.Setup(x => x.GetByIdAsync(evt.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(evt);

            var handler = new GetEventSectionsQueryHandler(_repositoryMock.Object);

            // Act
            var result = await handler.Handle(new GetEventSectionsQuery(evt.Id), CancellationToken.None);

            // Assert
            result.Should().HaveCount(1);
            result.ElementAt(0).Name.Should().Be("S1");
        }
    }
}
