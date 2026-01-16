using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using BookingMS.Shared.Events;
using UsersMS.Shared.Events;
using UsersMS.Domain.Entities;
using UsersMS.Domain.Interfaces;
using UsersMS.Infrastructure.Consumers;
using Xunit;

namespace UsersMS.Tests.Consumers
{
    public class BookingConsumersTests
    {
        private readonly Mock<IUserRepository> _repositoryMock;
        private readonly Mock<ILogger<BookingCreatedConsumer>> _loggerCreatedMock;

        private readonly Mock<IPublishEndpoint> _publishEndpointMock;
        private readonly Mock<ILogger<BookingConfirmedConsumer>> _loggerConfirmedMock;

        private readonly Mock<ILogger<BookingCancelledConsumer>> _loggerCancelledMock;

        public BookingConsumersTests()
        {
            _repositoryMock = new Mock<IUserRepository>();
            _loggerCreatedMock = new Mock<ILogger<BookingCreatedConsumer>>();
            
            _publishEndpointMock = new Mock<IPublishEndpoint>();
            _loggerConfirmedMock = new Mock<ILogger<BookingConfirmedConsumer>>();
            _loggerCancelledMock = new Mock<ILogger<BookingCancelledConsumer>>();
        }

        [Fact]
        public async Task BookingCreated_ShouldAddHistory()
        {
            
            var consumer = new BookingCreatedConsumer(_repositoryMock.Object, _loggerCreatedMock.Object);
            var userId = Guid.NewGuid();
            var message = new BookingCreatedEvent 
            { 
                BookingId = Guid.NewGuid(), 
                UserId = userId, 
                TotalAmount = 100m,
                Email = "test@test.com"
            };

            var contextMock = new Mock<ConsumeContext<BookingCreatedEvent>>();
            contextMock.Setup(x => x.Message).Returns(message);
            contextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

            _repositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User("Test", "test@test.com", "kc-1", UsersMS.Shared.Enums.UserRole.User));

            
            await consumer.Consume(contextMock.Object);

            
            _repositoryMock.Verify(x => x.AddHistoryAsync(It.Is<UserHistory>(h => h.UserId == userId && h.Action == "BookingCreated"), It.IsAny<CancellationToken>()), Times.Once);
            _repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task BookingConfirmed_ShouldPublishUserHistoryCreated()
        {
            
            var consumer = new BookingConfirmedConsumer(_publishEndpointMock.Object, _loggerConfirmedMock.Object);
            var userId = Guid.NewGuid();
            var message = new BookingConfirmedEvent 
            { 
                BookingId = Guid.NewGuid(), 
                UserId = userId, 
                EventId = Guid.NewGuid(), 
                EventName = "Event Name", 
                TotalAmount = 150m, 
                ConfirmedAt = DateTime.UtcNow, 
                Email = "test@test.com" 
            };

            var contextMock = new Mock<ConsumeContext<BookingConfirmedEvent>>();
            contextMock.Setup(x => x.Message).Returns(message);
            contextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

            
            await consumer.Consume(contextMock.Object);

            
            _publishEndpointMock.Verify(x => x.Publish(It.Is<UserHistoryCreatedEvent>(e => e.UserId == userId && e.Action == "BookingConfirmed"), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task BookingCancelled_ShouldPublishUserHistoryCreated()
        {
            
            var consumer = new BookingCancelledConsumer(_publishEndpointMock.Object, _loggerCancelledMock.Object);
            var userId = Guid.NewGuid();
            var message = new BookingCancelledEvent 
            { 
                BookingId = Guid.NewGuid(), 
                UserId = userId, 
                Reason = "Changed mind",
                Email = "test@test.com"
            };

            var contextMock = new Mock<ConsumeContext<BookingCancelledEvent>>();
            contextMock.Setup(x => x.Message).Returns(message);
            contextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

            
            await consumer.Consume(contextMock.Object);

            
            _publishEndpointMock.Verify(x => x.Publish(It.Is<UsersMS.Shared.Events.UserHistoryCreatedEvent>(e => e.UserId == userId && e.Action == "BookingCancelled"), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
