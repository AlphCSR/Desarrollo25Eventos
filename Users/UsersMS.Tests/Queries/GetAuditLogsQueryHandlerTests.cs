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
    public class GetAuditLogsQueryHandlerTests
    {
        [Fact]
        public async Task Handle_GetAuditLogs_Tests()
        {
            var auditServiceMock = new Mock<IAuditService>();
            var handler = new GetAuditLogsQueryHandler(auditServiceMock.Object);
            
            // Scenario: Successful retrieval
            var expectedLogs = new List<AuditLog> 
            { 
                new AuditLog { Action = "Test" },
                new AuditLog { Action = "Test 2" }
            };
            
            auditServiceMock.Setup(x => x.GetLatestLogsAsync(10)).ReturnsAsync(expectedLogs);
            
            var query = new GetAuditLogsQuery { Count = 10 };
            var result = await handler.Handle(query, CancellationToken.None);
            
            result.Should().BeEquivalentTo(expectedLogs);
            auditServiceMock.Verify(x => x.GetLatestLogsAsync(10), Times.Once);
        }
    }
}
