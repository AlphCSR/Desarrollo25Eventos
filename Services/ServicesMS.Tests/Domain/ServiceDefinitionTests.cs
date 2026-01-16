using System;
using ServicesMS.Domain.Entities;
using ServicesMS.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace ServicesMS.Tests.Domain
{
    public class ServiceDefinitionTests
    {
        [Fact]
        public void ServiceDefinition_Stock_Tests()
        {
            var eventId = Guid.NewGuid();
            
            var service = new ServiceDefinition("Catering", "Comida completa", 25.00m, true, 100, eventId);
            
            service.Name.Should().Be("Catering");
            service.Stock.Should().Be(100);

            service.CheckAvailability(50).Should().BeTrue();
            service.CheckAvailability(150).Should().BeFalse();

            service.ReduceStock(30);
            service.Stock.Should().Be(70);

            Action actReduceFail = () => service.ReduceStock(80);
            actReduceFail.Should().Throw<InsufficientStockException>();

            service.IncreaseStock(10);
            service.Stock.Should().Be(80);

            var infiniteService = new ServiceDefinition("Wifi", "Interet", 0, false, 0, eventId);
            infiniteService.CheckAvailability(9999).Should().BeTrue();
            infiniteService.ReduceStock(100);
            infiniteService.Stock.Should().Be(0);
        }
    }
}
