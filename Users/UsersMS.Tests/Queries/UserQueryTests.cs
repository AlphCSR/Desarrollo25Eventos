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

        [Fact]
        public async Task GetUserById_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User("User 1", "u1@test.com", "kc-1", UserRole.User);
            // Force ID to match (User ID is set in constructor but we can't set it directly easily without reflection or if it's protected set)
            // Actually User constructor generates a new ID. We should mock the repo to return this user when asked for *any* ID or the specific one.
            // But wait, the DTO returns the ID from the user object.
            
            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            var handler = new GetUserByIdQueryHandler(_userRepositoryMock.Object);

            // Act
            var result = await handler.Handle(new GetUserByIdQuery(userId), CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.Email.Should().Be("u1@test.com");
        }

        [Fact]
        public async Task GetUserById_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            var handler = new GetUserByIdQueryHandler(_userRepositoryMock.Object);

            // Act
            var result = await handler.Handle(new GetUserByIdQuery(userId), CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetUserByEmail_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var email = "u1@test.com";
            var user = new User("User 1", email, "kc-1", UserRole.User);
            
            _userRepositoryMock.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            var handler = new GetUserByEmailQueryHandler(_userRepositoryMock.Object);

            // Act
            var result = await handler.Handle(new GetUserByEmailQuery(email), CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.Email.Should().Be(email);
        }

        [Fact]
        public async Task GetUserByEmail_ShouldThrowUserNotFoundException_WhenUserDoesNotExist()
        {
            // Arrange
            var email = "unknown@test.com";
            _userRepositoryMock.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            var handler = new GetUserByEmailQueryHandler(_userRepositoryMock.Object);

            // Act
            Func<Task> act = async () => await handler.Handle(new GetUserByEmailQuery(email), CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<UserNotFoundException>()
                .WithMessage("User not found");
        }
    }
}
