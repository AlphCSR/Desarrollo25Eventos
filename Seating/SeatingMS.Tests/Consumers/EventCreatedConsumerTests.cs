using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using MassTransit;
using Microsoft.Extensions.Logging;
using SeatingMS.Infrastructure.Consumers;
using SeatingMS.Domain.Interfaces;
using SeatingMS.Domain.Entities;
using EventsMS.Shared.Events;

namespace SeatingMS.Tests.Consumers
{
    public class EventCreatedConsumerTests
    {
        private readonly Mock<IEventSeatRepository> _repoMock;
        private readonly Mock<ILogger<EventCreatedConsumer>> _loggerMock;
        private readonly EventCreatedConsumer _consumer;
        private readonly Mock<ConsumeContext<EventCreatedEvent>> _contextMock;

        public EventCreatedConsumerTests()
        {
            _repoMock = new Mock<IEventSeatRepository>();
            _loggerMock = new Mock<ILogger<EventCreatedConsumer>>();
            _consumer = new EventCreatedConsumer(_repoMock.Object, _loggerMock.Object);
            _contextMock = new Mock<ConsumeContext<EventCreatedEvent>>();
        }

        [Fact]
        public async Task Consume_ShouldCreateSeats_WhenEventHasSections()
        {
            var eventId = Guid.NewGuid();
            var sections = new List<SectionDto>
            {
                new SectionDto(Guid.NewGuid(), "VIP", 100, 20, true),
                new SectionDto(Guid.NewGuid(), "General", 50, 5, false)
            };

            var message = new EventCreatedEvent
            {
                EventId = eventId,
                Title = "Test Event",
                Sections = sections
            };

            _contextMock.Setup(c => c.Message).Returns(message);
            _contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

            _repoMock.Setup(r => r.GetByEventIdAsync(eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<EventSeat>());

            await _consumer.Consume(_contextMock.Object);

            _repoMock.Verify(r => r.AddRangeAsync(It.Is<IEnumerable<EventSeat>>(s => ((List<EventSeat>)s).Count == 25), It.IsAny<CancellationToken>()), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Consume_ShouldNotCreateSeats_WhenSeatsAlreadyExist()
        {
            var eventId = Guid.NewGuid();
            var sections = new List<SectionDto> { new SectionDto(Guid.NewGuid(), "VIP", 100, 20, true) };
            var message = new EventCreatedEvent { EventId = eventId, Sections = sections };

            _contextMock.Setup(c => c.Message).Returns(message);

            _repoMock.Setup(r => r.GetByEventIdAsync(eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<EventSeat> { new EventSeat(eventId, Guid.NewGuid(), "A", 1) });

            await _consumer.Consume(_contextMock.Object);

            _repoMock.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<EventSeat>>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
