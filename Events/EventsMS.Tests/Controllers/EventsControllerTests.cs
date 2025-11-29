using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using EventsMS.Controllers;
using EventsMS.Application.Commands.CreateEvent;
using EventsMS.Application.Commands.UpdateEvent;
using EventsMS.Application.Commands.DeleteEvent;
using EventsMS.Application.Queries.GetEvents;
using EventsMS.Application.Queries.GetEventById;
using EventsMS.Application.Queries.GetEventSections;
using EventsMS.Application.DTOs;
using EventsMS.Shared.Enums;

namespace EventsMS.Tests.Controllers
{
    public class EventsControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly EventsController _controller;

        public EventsControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new EventsController(_mediatorMock.Object);
        }

        [Fact]
        public async Task Create_ShouldReturnCreated_WhenSuccess()
        {
            // Arrange
            var dto = new CreateEventDto("Title", "Desc", DateTime.UtcNow, "Venue", "Cat", "Url", new List<CreateSectionDto>());
            var expectedId = Guid.NewGuid();
            _mediatorMock.Setup(x => x.Send(It.IsAny<CreateEventCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedId);

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.Value.Should().Be(expectedId);
        }

        [Fact]
        public async Task GetAll_ShouldReturnOk_WithList()
        {
            // Arrange
            var list = new List<EventDto> { new EventDto { Title = "E1" } };
            _mediatorMock.Setup(x => x.Send(It.IsAny<GetEventsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(list);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeSameAs(list);
        }

        [Fact]
        public async Task GetById_ShouldReturnOk_WhenFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            var dto = new EventDto { Id = id, Title = "E1" };
            _mediatorMock.Setup(x => x.Send(It.Is<GetEventByIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            // Act
            var result = await _controller.GetById(id);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeSameAs(dto);
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenNull()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mediatorMock.Setup(x => x.Send(It.IsAny<GetEventByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((EventDto?)null);

            // Act
            var result = await _controller.GetById(id);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetSections_ShouldReturnOk()
        {
            // Arrange
            var id = Guid.NewGuid();
            var list = new List<EventSectionDto>();
            _mediatorMock.Setup(x => x.Send(It.IsAny<GetEventSectionsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(list);

            // Act
            var result = await _controller.GetSections(id);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeSameAs(list);
        }

        [Fact]
        public async Task Update_ShouldReturnNoContent_WhenSuccess()
        {
            // Arrange
            var id = Guid.NewGuid();
            var dto = new UpdateEventDto("T", "D", DateTime.UtcNow, "V", "C");
            _mediatorMock.Setup(x => x.Send(It.IsAny<UpdateEventCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Update(id, dto);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task Delete_ShouldReturnNoContent_WhenSuccess()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mediatorMock.Setup(x => x.Send(It.IsAny<DeleteEventCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete(id);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }
        [Fact]
        public async Task Create_ShouldReturnBadRequest_WhenArgumentException()
        {
            // Arrange
            var dto = new CreateEventDto("Title", "Desc", DateTime.UtcNow, "Venue", "Cat", "Url", new List<CreateSectionDto>());
            _mediatorMock.Setup(x => x.Send(It.IsAny<CreateEventCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentException("Error"));

            // Act
            var result = await _controller.Create(dto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Update_ShouldReturnNotFound_WhenKeyNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            var dto = new UpdateEventDto("T", "D", DateTime.UtcNow, "V", "C");
            _mediatorMock.Setup(x => x.Send(It.IsAny<UpdateEventCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException());

            // Act
            var result = await _controller.Update(id, dto);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Update_ShouldReturnBadRequest_WhenArgumentException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var dto = new UpdateEventDto("T", "D", DateTime.UtcNow, "V", "C");
            _mediatorMock.Setup(x => x.Send(It.IsAny<UpdateEventCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentException("Error"));

            // Act
            var result = await _controller.Update(id, dto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Update_ShouldReturnBadRequest_WhenInvalidOperationException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var dto = new UpdateEventDto("T", "D", DateTime.UtcNow, "V", "C");
            _mediatorMock.Setup(x => x.Send(It.IsAny<UpdateEventCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Error"));

            // Act
            var result = await _controller.Update(id, dto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Delete_ShouldReturnNotFound_WhenKeyNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mediatorMock.Setup(x => x.Send(It.IsAny<DeleteEventCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException());

            // Act
            var result = await _controller.Delete(id);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Delete_ShouldReturnBadRequest_WhenInvalidOperationException()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mediatorMock.Setup(x => x.Send(It.IsAny<DeleteEventCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Error"));

            // Act
            var result = await _controller.Delete(id);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}
