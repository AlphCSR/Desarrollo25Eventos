using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UsersMS.Application.Commands;
using UsersMS.Application.Commands.UpdateUserPreferences;
using UsersMS.Application.DTOs;
using UsersMS.Domain.Entities;
using UsersMS.Domain.Exceptions;
using UsersMS.Domain.Interfaces;
using UsersMS.Shared.Enums;
using Xunit;

namespace UsersMS.Tests.Handlers
{
    public class UserPreferencesTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly UpdateUserPreferencesCommandHandler _handler;

        public UserPreferencesTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _handler = new UpdateUserPreferencesCommandHandler(_userRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldUpdatePreferences_WhenUserExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User("Test User", "test@test.com", "kc-123", UserRole.User);
            // Simulate existing user with empty preferences
            
            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            var newPreferences = new List<string> { "Theme:Dark", "Lang:ES" };
            var command = new UpdateUserPreferencesCommand(userId, new UpdateUserPreferencesDto(newPreferences));

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            user.Preferences.Should().BeEquivalentTo(newPreferences);
            _userRepositoryMock.Verify(x => x.AddHistoryAsync(It.IsAny<UserHistory>(), It.IsAny<CancellationToken>()), Times.Once);
            _userRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowUserNotFoundException_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            var command = new UpdateUserPreferencesCommand(userId, new UpdateUserPreferencesDto(new List<string>()));

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<UserNotFoundException>()
                .WithMessage($"Usuario con ID {userId} no encontrado.");
        }

        [Fact]
        public async Task Handle_ShouldNotAddHistory_WhenPreferencesAreUnchanged()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User("Test User", "test@test.com", "kc-123", UserRole.User);
            user.UpdatePreferences(new List<string> { "Theme:Dark" }); // Set initial state
            
            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            var command = new UpdateUserPreferencesCommand(userId, new UpdateUserPreferencesDto(new List<string> { "Theme:Dark" }));

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _userRepositoryMock.Verify(x => x.AddHistoryAsync(It.IsAny<UserHistory>(), It.IsAny<CancellationToken>()), Times.Never);
            _userRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
