using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using EventsMS.Application.Commands.UpdateEvent;
using EventsMS.Domain.Interfaces;
using EventsMS.Domain.Entities;
using EventsMS.Shared.Events;
using EventsMS.Shared.Enums;
using MassTransit;

namespace EventsMS.Tests.Application
{
    public class UpdateEventCommandHandlerTests
    {
        private readonly Mock<IEventRepository> _repoMock;
        private readonly Mock<IPublishEndpoint> _publishMock;
        private readonly UpdateEventCommandHandler _handler;

        public UpdateEventCommandHandlerTests()
        {
            _repoMock = new Mock<IEventRepository>();
            _publishMock = new Mock<IPublishEndpoint>();
            _handler = new UpdateEventCommandHandler(_repoMock.Object, _publishMock.Object);
        }

        [Fact]
        public async Task Handle_ExistingEvent_ShouldUpdateAndPublish()
        {
            
            var eventId = Guid.NewGuid();
            var idUser = Guid.NewGuid();
            var existingEvent = new EventsMS.Domain.Entities.Event(idUser, "Old Title", "Old Desc", DateTime.UtcNow.AddDays(10), DateTime.UtcNow.AddDays(11), "Venue", new List<string> { "OldCat" });

            _repoMock.Setup(r => r.GetByIdAsync(existingEvent.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingEvent);

            var command = new UpdateEventCommand(
                existingEvent.Id,
                "New Title",
                "New Desc",
                DateTime.UtcNow.AddDays(20),

                DateTime.UtcNow.AddDays(21),
                "New Venue",
                new List<string> { "NewCat" },
                EventType.Physical,
                null
            );

            
            await _handler.Handle(command, CancellationToken.None);

            
            Assert.Equal("New Title", existingEvent.Title);
            Assert.Equal(new List<string> { "NewCat" }, existingEvent.Categories);

            _repoMock.Verify(r => r.UpdateAsync(existingEvent, It.IsAny<CancellationToken>()), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            _publishMock.Verify(p => p.Publish(It.Is<EventUpdatedEvent>(e => 
                e.EventId == existingEvent.Id &&
                e.IdUser == idUser &&
                e.Title == "New Title"), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NonExistingEvent_ShouldThrowException()
        {
            
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((EventsMS.Domain.Entities.Event)null);

            var command = new UpdateEventCommand(Guid.NewGuid(), "T", "D", DateTime.UtcNow, DateTime.UtcNow.AddDays(1), "V", new List<string>{"C"}, EventType.Physical, null);

            
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        }
    }
}
