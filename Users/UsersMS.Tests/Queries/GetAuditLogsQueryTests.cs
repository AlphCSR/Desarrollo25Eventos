using FluentAssertions;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UsersMS.Application.Interfaces;
using UsersMS.Application.Queries;
using UsersMS.Domain.Entities;
using Xunit;

namespace UsersMS.Tests.Queries
{
    public class GetAuditLogsQueryTests
    {
        private readonly Mock<IAuditService> _auditServiceMock;
        private readonly GetAuditLogsQueryHandler _handler;

        public GetAuditLogsQueryTests()
        {
            _auditServiceMock = new Mock<IAuditService>();
            _handler = new GetAuditLogsQueryHandler(_auditServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnLogs_FromService()
        {
            
            var count = 10;
            var expectedLogs = new List<AuditLog>
            {
                new AuditLog { Action = "Test1" },
                new AuditLog { Action = "Test2" }
            };

            _auditServiceMock.Setup(x => x.GetLatestLogsAsync(count))
                .ReturnsAsync(expectedLogs);

            var query = new GetAuditLogsQuery { Count = count };

            
            var result = await _handler.Handle(query, CancellationToken.None);

            
            result.Should().BeEquivalentTo(expectedLogs);
            _auditServiceMock.Verify(x => x.GetLatestLogsAsync(count), Times.Once);
        }
    }
}
