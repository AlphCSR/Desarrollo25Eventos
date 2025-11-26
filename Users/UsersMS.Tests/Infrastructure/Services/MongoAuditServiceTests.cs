using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using UsersMS.Domain.Entities;
using Xunit;

namespace UsersMS.Tests.Infrastructure.Services
{
    public class MongoAuditServiceTests
    {
        private readonly Mock<IMongoClient> _mongoClientMock;
        private readonly Mock<IMongoDatabase> _mongoDatabaseMock;
        private readonly Mock<IMongoCollection<AuditLog>> _mongoCollectionMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly MongoAuditService _service;

        public MongoAuditServiceTests()
        {
            _mongoClientMock = new Mock<IMongoClient>();
            _mongoDatabaseMock = new Mock<IMongoDatabase>();
            _mongoCollectionMock = new Mock<IMongoCollection<AuditLog>>();
            _configurationMock = new Mock<IConfiguration>();

            _configurationMock.Setup(c => c["MongoDb:DatabaseName"]).Returns("TestDb");

            _mongoClientMock.Setup(c => c.GetDatabase(It.IsAny<string>(), It.IsAny<MongoDatabaseSettings>()))
                .Returns(_mongoDatabaseMock.Object);

            _mongoDatabaseMock.Setup(d => d.GetCollection<AuditLog>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()))
                .Returns(_mongoCollectionMock.Object);

            _service = new MongoAuditService(_mongoClientMock.Object, _configurationMock.Object);
        }

        [Fact]
        public async Task LogAsync_ShouldInsertLogIntoCollection()
        {
            // Arrange
            var log = new AuditLog 
            { 
                UserId = "User", 
                Action = "Action", 
                Payload = "Details" 
            };

            // Act
            await _service.LogAsync(log);

            // Assert
            _mongoCollectionMock.Verify(
                c => c.InsertOneAsync(
                    log,
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
