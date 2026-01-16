using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Moq;
using ServicesMS.Application.Commands;
using ServicesMS.Application.DTOs;
using ServicesMS.Domain.Interfaces;
using ServicesMS.Domain.Entities;

namespace ServicesMS.Tests.Application
{
    public class CreateServiceCommandHandlerTests
    {
        private readonly Mock<IServiceRepository> _repoMock;
        private readonly CreateServiceCommandHandler _handler;

        public CreateServiceCommandHandlerTests()
        {
            _repoMock = new Mock<IServiceRepository>();
            _handler = new CreateServiceCommandHandler(_repoMock.Object);
        }

        [Fact]
        public async Task Handle_CreateService_ShouldReturnId()
        {
            var dto = new CreateServiceDto("VIP Parking", "Includes valet", 25.0m, Guid.NewGuid(), true, 50);
            var command = new CreateServiceCommand(dto);

            _repoMock.Setup(r => r.AddDefinitionAsync(It.IsAny<ServiceDefinition>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Should().NotBeEmpty();
            _repoMock.Verify(r => r.AddDefinitionAsync(It.Is<ServiceDefinition>(s => s.Name == "VIP Parking"), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
