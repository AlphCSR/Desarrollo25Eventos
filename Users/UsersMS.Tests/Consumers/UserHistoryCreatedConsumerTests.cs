using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using UsersMS.Domain.Entities;
using UsersMS.Domain.Interfaces;
using UsersMS.Infrastructure.Consumers;
using UsersMS.Shared.Events;
using Xunit;
using System;
using System.Text.Json;

namespace UsersMS.Tests.Consumers
{
    public class UserHistoryCreatedConsumerTests
    {
        private readonly Mock<IUserRepository> _repositoryMock;
        private readonly Mock<ILogger<UserHistoryCreatedConsumer>> _loggerMock;
        private readonly UserHistoryCreatedConsumer _consumer;

        public UserHistoryCreatedConsumerTests()
        {
            _repositoryMock = new Mock<IUserRepository>();
            _loggerMock = new Mock<ILogger<UserHistoryCreatedConsumer>>();
            _consumer = new UserHistoryCreatedConsumer(_repositoryMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Consume_Tests()
        {
            // --- SCENARIO 1: Successful history creation (Standard) ---
            var userId = Guid.NewGuid();
            var message1 = new UserHistoryCreatedEvent(userId, "Login", "User logged in", DateTime.UtcNow);
            var contextMock1 = new Mock<ConsumeContext<UserHistoryCreatedEvent>>();
            contextMock1.Setup(x => x.Message).Returns(message1);
            _repositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User("Juan Perez", "juan@test.com", "kc-1", UsersMS.Shared.Enums.UserRole.User));

            await _consumer.Consume(contextMock1.Object);
            _repositoryMock.Verify(x => x.AddHistoryAsync(It.Is<UserHistory>(h => h.Action == "Login"), It.IsAny<CancellationToken>()), Times.Once);

            // --- SCENARIO 2: Sanitization (Redacting password) ---
            var jsonWithPassword = "{\"Username\": \"juan\", \"Password\": \"secret123\", \"Nested\": {\"OldPassword\": \"old123\"}}";
            var message2 = new UserHistoryCreatedEvent(userId, "ChangePassword", jsonWithPassword, DateTime.UtcNow);
            var contextMock2 = new Mock<ConsumeContext<UserHistoryCreatedEvent>>();
            contextMock2.Setup(x => x.Message).Returns(message2);

            await _consumer.Consume(contextMock2.Object);
            _repositoryMock.Verify(x => x.AddHistoryAsync(It.Is<UserHistory>(h => 
                h.Action == "ChangePassword" && 
                h.Details.Contains("***REDACTED***") && 
                !h.Details.Contains("secret123") &&
                !h.Details.Contains("old123")), 
                It.IsAny<CancellationToken>()), Times.Once);

            // --- SCENARIO 3: User Not Found (Logging Warning) ---
            var unknownUserId = Guid.NewGuid();
            var message3 = new UserHistoryCreatedEvent(unknownUserId, "UnknownAction", "Details", DateTime.UtcNow);
            var contextMock3 = new Mock<ConsumeContext<UserHistoryCreatedEvent>>();
            contextMock3.Setup(x => x.Message).Returns(message3);
            _repositoryMock.Setup(x => x.GetByIdAsync(unknownUserId, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
            _repositoryMock.Setup(x => x.GetByKeycloakIdAsync(unknownUserId.ToString(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

            await _consumer.Consume(contextMock3.Object);
            // Verify no history was added for this call
            _repositoryMock.Verify(x => x.AddHistoryAsync(It.Is<UserHistory>(h => h.UserId == unknownUserId), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
