using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using UsersMS.Application.Behaviors;
using UsersMS.Application.Interfaces;
using UsersMS.Domain.Entities;
using Xunit;

namespace UsersMS.Tests.Behaviors
{
    public class AuditBehaviorTests
    {
        private readonly Mock<IAuditService> _auditServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly AuditBehavior<TestCommand, string> _behavior;

        public AuditBehaviorTests()
        {
            _auditServiceMock = new Mock<IAuditService>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _behavior = new AuditBehavior<TestCommand, string>(_auditServiceMock.Object, _httpContextAccessorMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldLogSuccess_WhenNextDelegateSucceeds()
        {
            
            var command = new TestCommand();
            var userId = "user-123";
            SetupHttpContext(userId);

            RequestHandlerDelegate<string> next = () => Task.FromResult("Success");

            
            var result = await _behavior.Handle(command, next, CancellationToken.None);

            
            result.Should().Be("Success");
            _auditServiceMock.Verify(x => x.LogAsync(It.Is<AuditLog>(l => 
                l.UserId == userId && 
                l.Action == nameof(TestCommand) && 
                l.IsSuccess == true &&
                l.ErrorMessage == null
            )), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldLogFailure_WhenNextDelegateThrows()
        {
            
            var command = new TestCommand();
            var userId = "user-123";
            SetupHttpContext(userId);
            var exception = new Exception("Something went wrong");

            RequestHandlerDelegate<string> next = () => throw exception;

            
            Func<Task> act = async () => await _behavior.Handle(command, next, CancellationToken.None);

            
            await act.Should().ThrowAsync<Exception>().WithMessage("Something went wrong");
            _auditServiceMock.Verify(x => x.LogAsync(It.Is<AuditLog>(l => 
                l.UserId == userId && 
                l.Action == nameof(TestCommand) && 
                l.IsSuccess == false &&
                l.ErrorMessage == "Something went wrong"
            )), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldUseAnonymousUser_WhenHttpContextIsNull()
        {
            
            var command = new TestCommand();
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

            RequestHandlerDelegate<string> next = () => Task.FromResult("Success");

            
            await _behavior.Handle(command, next, CancellationToken.None);

            
            _auditServiceMock.Verify(x => x.LogAsync(It.Is<AuditLog>(l => l.UserId == "Anonymous")), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldUseAnonymousUser_WhenUserPrincipalHasNoSubClaim()
        {
            
            var command = new TestCommand();
            var context = new DefaultHttpContext();
            var identity = new ClaimsIdentity(); 
            context.User = new ClaimsPrincipal(identity);
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

            RequestHandlerDelegate<string> next = () => Task.FromResult("Success");

            
            await _behavior.Handle(command, next, CancellationToken.None);

            
            _auditServiceMock.Verify(x => x.LogAsync(It.Is<AuditLog>(l => l.UserId == "Anonymous")), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldNotLog_WhenRequestIsNotCommand()
        {
            
            var query = new TestQuery();
            var behavior = new AuditBehavior<TestQuery, string>(_auditServiceMock.Object, _httpContextAccessorMock.Object);
            RequestHandlerDelegate<string> next = () => Task.FromResult("Success");

            
            await behavior.Handle(query, next, CancellationToken.None);

            
            _auditServiceMock.Verify(x => x.LogAsync(It.IsAny<AuditLog>()), Times.Never);
        }

        private void SetupHttpContext(string userId)
        {
            var context = new DefaultHttpContext();
            var claims = new[] { new Claim("sub", userId) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            context.User = new ClaimsPrincipal(identity);
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);
        }

        public class TestCommand : IRequest<string> { }
        public class TestQuery : IRequest<string> { }
    }
}
