using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Moq;
using ServicesMS.Application.Queries;
using ServicesMS.Domain.Interfaces;
using ServicesMS.Domain.Entities;
using ServicesMS.Application.DTOs;

namespace ServicesMS.Tests.Application
{
    public class GetServicesByEventQueryHandlerTests
    {
        private readonly Mock<IServiceRepository> _repoMock;
        private readonly GetServicesByEventQueryHandler _handler;

        public GetServicesByEventQueryHandlerTests()
        {
            _repoMock = new Mock<IServiceRepository>();
            _handler = new GetServicesByEventQueryHandler(_repoMock.Object);
        }

        [Fact]
        public async Task Handle_GetServices_ShouldReturnList()
        {
            var eventId = Guid.NewGuid();
            var services = new List<ServiceDefinition>
            {
                new ServiceDefinition("Valet", "Parking", 15.0m, false, 0, eventId),
                new ServiceDefinition("Dinner", "Food", 50.0m, true, 100, eventId)
            };

            _repoMock.Setup(r => r.GetDefinitionsByEventAsync(eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(services);

            var query = new GetServicesByEventQuery(eventId);
            var result = await _handler.Handle(query, CancellationToken.None);

            result.Should().HaveCount(2);
            result.Should().Contain(s => s.Name == "Valet");
        }
    }
}
