using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using EventsMS.Application.Commands.CreateEvent;
using EventsMS.Application.Commands.UpdateEvent;
using EventsMS.Application.Commands.DeleteEvent;
using EventsMS.Application.DTOs;
using EventsMS.Domain.Entities;
using EventsMS.Domain.Interfaces;
using EventsMS.Shared.Enums;
using EventsMS.Shared.Events;
using MassTransit;
using System.Collections.Generic;
using EventEntity = EventsMS.Domain.Entities.Event;

namespace EventsMS.Tests.Handlers
{
    public class EventHandlerTests
    {
        private readonly Mock<IEventRepository> _eventRepositoryMock;
        private readonly Mock<IPublishEndpoint> _publishEndpointMock;

        public EventHandlerTests()
        {
            _eventRepositoryMock = new Mock<IEventRepository>();
            _publishEndpointMock = new Mock<IPublishEndpoint>();
        }

        // =================================================================================================
        // 1. CREATE EVENT
        // =================================================================================================

        [Fact]
        public async Task Handle_CreateEvent_Success()
        {
            // ARRANGE
            var dto = new CreateEventDto(
                "Test Event",
                "Description",
                DateTime.UtcNow.AddDays(10),
                "Venue",
                "Concert",
                "http://image.com",
                new List<CreateSectionDto>()
            );

            var handler = new CreateEventCommandHandler(_eventRepositoryMock.Object, _publishEndpointMock.Object);
            var command = new CreateEventCommand(dto);

            // ACT
            var result = await handler.Handle(command, CancellationToken.None);

            // ASSERT
            result.Should().NotBeEmpty();
            _eventRepositoryMock.Verify(x => x.AddAsync(It.IsAny<EventEntity>(), It.IsAny<CancellationToken>()), Times.Once);
            _eventRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _publishEndpointMock.Verify(x => x.Publish(It.IsAny<EventCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_CreateEvent_InvalidData_ShouldThrowException()
        {
            // ARRANGE
            var dto = new CreateEventDto(
                "", // Empty Title
                "Description",
                DateTime.UtcNow.AddDays(10),
                "Venue",
                "Concert",
                "http://image.com",
                new List<CreateSectionDto>()
            );

            var handler = new CreateEventCommandHandler(_eventRepositoryMock.Object, _publishEndpointMock.Object);
            var command = new CreateEventCommand(dto);

            // ACT & ASSERT
            await Assert.ThrowsAsync<EventsMS.Domain.Exceptions.InvalidEventDataException>(() => 
                handler.Handle(command, CancellationToken.None));
        }

        // =================================================================================================
        // 2. UPDATE EVENT
        // =================================================================================================

        [Fact]
        public async Task Handle_UpdateEvent_Success()
        {
            // ARRANGE
            var eventId = Guid.NewGuid();
            var existingEvent = new EventEntity("Old Title", "Old Desc", DateTime.UtcNow.AddDays(5), "Old Venue", "Old Cat");
            // Reflection to set ID if needed, or assume repository returns it correctly mapped. 
            // Since ID is private set and generated in constructor, we rely on the object reference or we can use reflection to set it if strictly needed for the test to match IDs, 
            // but for this unit test, the repository returning the object is enough.

            _eventRepositoryMock.Setup(x => x.GetByIdAsync(eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingEvent);

            var handler = new UpdateEventCommandHandler(_eventRepositoryMock.Object);
            var command = new UpdateEventCommand(eventId, "New Title", "New Desc", DateTime.UtcNow.AddDays(10), "New Venue", "New Cat");

            // ACT
            await handler.Handle(command, CancellationToken.None);

            // ASSERT
            existingEvent.Title.Should().Be("New Title");
            existingEvent.Category.Should().Be("New Cat");
            _eventRepositoryMock.Verify(x => x.UpdateAsync(existingEvent, It.IsAny<CancellationToken>()), Times.Once);
            _eventRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_UpdateEvent_NotFound_ShouldThrowException()
        {
            // ARRANGE
            var eventId = Guid.NewGuid();
            _eventRepositoryMock.Setup(x => x.GetByIdAsync(eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((EventEntity?)null);

            var handler = new UpdateEventCommandHandler(_eventRepositoryMock.Object);
            var command = new UpdateEventCommand(eventId, "New Title", "New Desc", DateTime.UtcNow.AddDays(10), "New Venue", "New Cat");

            // ACT
            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            // ASSERT
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        // =================================================================================================
        // 3. DELETE EVENT
        // =================================================================================================

        [Fact]
        public async Task Handle_DeleteEvent_Success()
        {
            // ARRANGE
            var eventId = Guid.NewGuid();
            var existingEvent = new EventEntity("To Delete", "Desc", DateTime.UtcNow.AddDays(5), "Venue", "Cat");
            
            _eventRepositoryMock.Setup(x => x.GetByIdAsync(eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingEvent);

            var handler = new DeleteEventCommandHandler(_eventRepositoryMock.Object);
            var command = new DeleteEventCommand(eventId);

            // ACT
            await handler.Handle(command, CancellationToken.None);

            // ASSERT
            existingEvent.Status.Should().Be(EventStatus.Cancelled);
            _eventRepositoryMock.Verify(x => x.UpdateAsync(existingEvent, It.IsAny<CancellationToken>()), Times.Once);
            _eventRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
