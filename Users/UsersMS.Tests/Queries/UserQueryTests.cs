using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UsersMS.Application.Queries.GetAllUsers;
using UsersMS.Application.Queries.GetUserByEmail;
using UsersMS.Application.Queries.GetUserById;
using UsersMS.Domain.Entities;
using UsersMS.Domain.Exceptions;
using UsersMS.Domain.Interfaces;
using UsersMS.Shared.Enums;
using Xunit;

namespace UsersMS.Tests.Queries
{
    public class UserQueryTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;

        public UserQueryTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
        }

        [Fact]
        public async Task GetAllUsers_ShouldReturnAllUsers()
        {
            // Arrange
            var users = new List<User>
            {
                new User("User 1", "u1@test.com", "kc-1", UserRole.User),
                new User("User 2", "u2@test.com", "kc-2", UserRole.Admin)
            };
            _userRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(users);

            var handler = new GetAllUsersQueryHandler(_userRepositoryMock.Object);

            // Act
            var result = await handler.Handle(new GetAllUsersQuery(), CancellationToken.None);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(u => u.Email == "u1@test.com");
            result.Should().Contain(u => u.Email == "u2@test.com");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GetUserById_ShouldReturnExpectedResult(bool userExists)
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = userExists ? new User("User 1", "u1@test.com", "kc-1", UserRole.User) : null;

            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            var handler = new GetUserByIdQueryHandler(_userRepositoryMock.Object);

            // Act
            var result = await handler.Handle(new GetUserByIdQuery(userId), CancellationToken.None);

            // Assert
            if (userExists)
            {
                result.Should().NotBeNull();
                result!.Email.Should().Be("u1@test.com");
            }
            else
            {
                result.Should().BeNull();
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GetUserByEmail_ShouldReturnExpectedResult(bool userExists)
        {
            // Arrange
            var email = userExists ? "u1@test.com" : "unknown@test.com";
            var user = userExists ? new User("User 1", email, "kc-1", UserRole.User) : null;
            
            _userRepositoryMock.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            var handler = new GetUserByEmailQueryHandler(_userRepositoryMock.Object);

            // Act & Assert
            if (userExists)
            {
                var result = await handler.Handle(new GetUserByEmailQuery(email), CancellationToken.None);
                result.Should().NotBeNull();
                result!.Email.Should().Be(email);
            }
            else
            {
                Func<Task> act = async () => await handler.Handle(new GetUserByEmailQuery(email), CancellationToken.None);
                await act.Should().ThrowAsync<UserNotFoundException>()
                    .WithMessage("User not found");
            }
        }
    }
}
