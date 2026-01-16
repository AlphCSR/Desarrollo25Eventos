using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Moq;
using EventsMS.Application.Commands.CreateEvent;
using EventsMS.Application.DTOs;
using EventsMS.Domain.Interfaces;
using EventsMS.Domain.Entities;
using EventsMS.Shared.Events;
using EventsMS.Shared.Enums;
using EventsMS.Domain.Exceptions;
using MassTransit;

namespace EventsMS.Tests.Application
{
    public class CreateEventCommandHandlerTests
    {
        private readonly Mock<IEventRepository> _repoMock;
        private readonly Mock<IPublishEndpoint> _publishMock;
        private readonly CreateEventCommandHandler _handler;

        public CreateEventCommandHandlerTests()
        {
            _repoMock = new Mock<IEventRepository>();
            _publishMock = new Mock<IPublishEndpoint>();
            _handler = new CreateEventCommandHandler(_repoMock.Object, _publishMock.Object);
        }

        [Fact]
        public async Task Handle_CreateEvent_Tests()
        {
            var dtoSuccess = new CreateEventDto(
                Guid.NewGuid(),
                "Title Success",
                "Desc",
                DateTime.UtcNow.AddDays(5),
                DateTime.UtcNow.AddDays(6),
                "Venue",
                new List<string> { "Music" },
                "http://image.url",
                EventType.Physical,
                null,
                new List<CreateSectionDto> { new CreateSectionDto("VIP", 100, 50, true) }
            );
            var commandSuccess = new CreateEventCommand(dtoSuccess);

            var result = await _handler.Handle(commandSuccess, CancellationToken.None);

            result.Should().NotBeEmpty();
            _repoMock.Verify(r => r.AddAsync(It.IsAny<EventsMS.Domain.Entities.Event>(), It.IsAny<CancellationToken>()), Times.Once);
            _publishMock.Verify(p => p.Publish(It.IsAny<EventCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);

            var dtoFail = new CreateEventDto(
                Guid.NewGuid(),
                "",
                "Desc",
                DateTime.UtcNow.AddDays(5),
                DateTime.UtcNow.AddDays(6),
                "Venue",
                new List<string> { "Music" },
                null, EventType.Physical, null, new List<CreateSectionDto>()
            );
            var commandFail = new CreateEventCommand(dtoFail);

            Func<Task> act = async () => await _handler.Handle(commandFail, CancellationToken.None);
            
            await act.Should().ThrowAsync<InvalidEventDataException>()
                     .WithMessage("El título del evento no puede estar vacío.");

            _repoMock.Verify(r => r.AddAsync(It.Is<EventsMS.Domain.Entities.Event>(e => e.Title == ""), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
