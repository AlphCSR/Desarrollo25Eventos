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
using UsersMS.Shared.Enums;
using UsersMS.Application.Queries.GetUserHistory;
using UsersMS.Application.DTOs;
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
            var users = new List<User>
            {
                new User("User 1", "u1@test.com", "kc-1", UserRole.User),
                new User("User 2", "u2@test.com", "kc-2", UserRole.Admin)
            };
            _userRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(users);

            var handler = new GetAllUsersQueryHandler(_userRepositoryMock.Object);

            var result = await handler.Handle(new GetAllUsersQuery(), CancellationToken.None);

            result.Should().HaveCount(2);
            result.Should().Contain(u => u.Email == "u1@test.com");
            result.Should().Contain(u => u.Email == "u2@test.com");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GetUserById_ShouldReturnExpectedResult(bool userExists)
        {
            var userId = Guid.NewGuid();
            var user = userExists ? new User("User 1", "u1@test.com", "kc-1", UserRole.User) : null;

            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            var handler = new GetUserByIdQueryHandler(_userRepositoryMock.Object);

            var result = await handler.Handle(new GetUserByIdQuery(userId), CancellationToken.None);

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
            var email = userExists ? "u1@test.com" : "unknown@test.com";
            var user = userExists ? new User("User 1", email, "kc-1", UserRole.User) : null;
            
            _userRepositoryMock.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            var handler = new GetUserByEmailQueryHandler(_userRepositoryMock.Object);

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


        [Fact]
        public async Task GetUserHistory_ShouldReturnHistory_WhenUserExists()
        {
            var userId = Guid.NewGuid();
            var user = new User("User with History", "hist@test.com", "kc-hist", UserRole.User);
            
            var historyItem1 = new UserHistory(userId, "UserCreated", "Usuario creado", DateTime.UtcNow);
            var historyItem2 = new UserHistory(userId, "Login", "User logged in", DateTime.UtcNow);
            
            var historyField = typeof(User).GetField("_history", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if(historyField != null)
            {
                var list = (List<UserHistory>)historyField.GetValue(user);
                list.Add(historyItem1);
                list.Add(historyItem2);
            }
            
            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            var handler = new GetUserHistoryQueryHandler(_userRepositoryMock.Object);

            var result = await handler.Handle(new GetUserHistoryQuery(userId), CancellationToken.None);

            result.Should().HaveCount(2);
            result.Should().Contain(h => h.Action == "UserCreated");
            result.Should().Contain(h => h.Action == "Login");
        }
    }
}
