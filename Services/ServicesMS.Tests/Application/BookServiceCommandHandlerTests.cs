using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MassTransit;
using Moq;
using ServicesMS.Application.Commands;
using ServicesMS.Application.DTOs;
using ServicesMS.Domain.Entities;
using ServicesMS.Domain.Interfaces;
using ServicesMS.Shared.Events;
using Xunit;

namespace ServicesMS.Tests.Application
{
    public class BookServiceCommandHandlerTests
    {
        private readonly Mock<IServiceRepository> _repositoryMock;
        private readonly Mock<IPublishEndpoint> _publishEndpointMock;

        public BookServiceCommandHandlerTests()
        {
            _repositoryMock = new Mock<IServiceRepository>();
            _publishEndpointMock = new Mock<IPublishEndpoint>();
        }

        [Fact]
        public async Task Handle_BookService_Tests()
        {
            var handler = new BookServiceCommandHandler(_repositoryMock.Object, _publishEndpointMock.Object);
            var serviceId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();
            
            var service = new ServiceDefinition("Test", "Desc", 10, true, 5, Guid.NewGuid());
            typeof(ServiceDefinition).GetProperty("Id")?.SetValue(service, serviceId);

            var dto = new BookServiceDto(serviceId, userId, bookingId, 2);
            var command = new BookServiceCommand(dto);

            _repositoryMock.Setup(x => x.GetDefinitionByIdAsync(serviceId, It.IsAny<CancellationToken>())).ReturnsAsync(service);
            
            var result = await handler.Handle(command, CancellationToken.None);

            result.Should().BeTrue();
            service.Stock.Should().Be(3);
            _repositoryMock.Verify(x => x.AddBookingAsync(It.IsAny<ServiceBooking>(), It.IsAny<CancellationToken>()), Times.Once);
            _publishEndpointMock.Verify(x => x.Publish(It.IsAny<ServiceBookedEvent>(), It.IsAny<CancellationToken>()), Times.Once);

            var dtoFail = new BookServiceDto(serviceId, userId, bookingId, 10);
            var commandFail = new BookServiceCommand(dtoFail);
            
            Func<Task> act = async () => await handler.Handle(commandFail, CancellationToken.None);
            await act.Should().ThrowAsync<Exception>().WithMessage("No hay suficiente stock");

            _repositoryMock.Setup(x => x.GetDefinitionByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((ServiceDefinition?)null);
            
            Func<Task> act2 = async () => await handler.Handle(new BookServiceCommand(new BookServiceDto(Guid.NewGuid(), userId, bookingId, 1)), CancellationToken.None);
            await act2.Should().ThrowAsync<Exception>().WithMessage("Servicio no encontrado");
        }
    }
}
