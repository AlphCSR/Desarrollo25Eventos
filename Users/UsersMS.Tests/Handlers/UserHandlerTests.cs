using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using UsersMS.Application.Commands.CreateUser;
using UsersMS.Application.Commands.UpdateUser;
using UsersMS.Application.Commands.DeleteUser;
using UsersMS.Application.DTOs;
using UsersMS.Application.Interfaces;
using UsersMS.Domain.Entities;
using UsersMS.Domain.Interfaces;
using UsersMS.Domain.Exceptions;
using UsersMS.Shared.Enums;
using System.Collections.Generic;

namespace UsersMS.Tests.Handlers
{
    public class UsersHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IKeycloakService> _keycloakServiceMock;
        private readonly Mock<IAuditService> _auditServiceMock;

        public UsersHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _keycloakServiceMock = new Mock<IKeycloakService>();
            _auditServiceMock = new Mock<IAuditService>();
        }

        // =================================================================================================
        // 1. CREATE USER
        // =================================================================================================

        [Theory]
        [MemberData(nameof(GetCreateUserSuccessScenarios))]
        public async Task Handle_CreateUser_Success(string scenarioName, CreateUserDto dto)
        {
            // ARRANGE
            _userRepositoryMock.Setup(x => x.GetByEmailAsync(dto.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null); // No existe

            _keycloakServiceMock.Setup(x => x.RegisterUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("keycloak-id-123");

            _keycloakServiceMock.Setup(x => x.AssignRoleAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var handler = new CreateUserCommandHandler(_userRepositoryMock.Object, _keycloakServiceMock.Object);
            var command = new CreateUserCommand(dto);

            // ACT
            var result = await handler.Handle(command, CancellationToken.None);

            // ASSERT
            result.Should().NotBeEmpty();
            _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
            _keycloakServiceMock.Verify(x => x.RegisterUserAsync(dto.Email, dto.Password, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            _keycloakServiceMock.Verify(x => x.AssignRoleAsync(dto.Email, dto.Role.ToString(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Theory]
        [MemberData(nameof(GetCreateUserFailureScenarios))]
        public async Task Handle_CreateUser_Failure(string scenarioName, CreateUserDto dto, User? existingUserInDb, Type expectedExceptionType, string expectedErrorMessage)
        {
            // ARRANGE
            _userRepositoryMock.Setup(x => x.GetByEmailAsync(dto.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingUserInDb);

            var handler = new CreateUserCommandHandler(_userRepositoryMock.Object, _keycloakServiceMock.Object);
            var command = new CreateUserCommand(dto);

            // ACT
            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            // ASSERT
            await act.Should().ThrowAsync<Exception>()
                .Where(e => e.GetType() == expectedExceptionType)
                .WithMessage($"*{expectedErrorMessage}*");

            _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        public static IEnumerable<object?[]> GetCreateUserSuccessScenarios()
        {
            yield return new object?[]
            {
                "Success_ValidUser",
                new CreateUserDto("Juan Perez", "juan@test.com", "123456", UserRole.User)
            };
        }

        public static IEnumerable<object?[]> GetCreateUserFailureScenarios()
        {
            yield return new object?[]
            {
                "Fail_DuplicateEmail",
                new CreateUserDto("Pedro Dup", "existe@test.com", "123456", UserRole.User),
                new User("Old User", "existe@test.com", "k-id", UserRole.User),
                typeof(UserAlreadyExistsException),
                "ya existe"
            };
        }

        // =================================================================================================
        // 2. UPDATE USER
        // =================================================================================================

        [Theory]
        [MemberData(nameof(GetUpdateUserSuccessScenarios))]
        public async Task Handle_UpdateUser_Success(string scenarioName, Guid targetId, User userFoundInDb, UpdateUserDto dto)
        {
            // ARRANGE
            _userRepositoryMock.Setup(x => x.GetByIdAsync(targetId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userFoundInDb);

            var handler = new UpdateUserCommandHandler(_userRepositoryMock.Object, _keycloakServiceMock.Object, _auditServiceMock.Object);
            var command = new UpdateUserCommand(targetId, dto);

            // ACT
            await handler.Handle(command, CancellationToken.None);

            // ASSERT
            _keycloakServiceMock.Verify(x => x.UpdateUserAsync(userFoundInDb.KeycloakId, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            _userRepositoryMock.Verify(x => x.AddHistoryAsync(It.IsAny<UserHistory>(), It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(x => x.LogAsync(It.Is<AuditLog>(l => l.Action == "UpdateUser")), Times.Once);
        }

        [Theory]
        [MemberData(nameof(GetUpdateUserFailureScenarios))]
        public async Task Handle_UpdateUser_Failure(string scenarioName, Guid targetId, UpdateUserDto dto, Type expectedExceptionType)
        {
            // ARRANGE
            _userRepositoryMock.Setup(x => x.GetByIdAsync(targetId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            var handler = new UpdateUserCommandHandler(_userRepositoryMock.Object, _keycloakServiceMock.Object, _auditServiceMock.Object);
            var command = new UpdateUserCommand(targetId, dto);

            // ACT
            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            // ASSERT
            await act.Should().ThrowAsync<Exception>()
               .Where(e => e.GetType() == expectedExceptionType);
        }

        public static IEnumerable<object?[]> GetUpdateUserSuccessScenarios()
        {
            var validId = Guid.NewGuid();
            var existingUser = new User("Original Name", "test@test.com", "kc-123", UserRole.User);

            yield return new object?[]
            {
                "Success_UpdateProfile",
                validId,
                existingUser,
                new UpdateUserDto("Nuevo Nombre")
            };
        }

        public static IEnumerable<object?[]> GetUpdateUserFailureScenarios()
        {
            yield return new object?[]
            {
                "Fail_UserNotFound",
                Guid.NewGuid(),
                new UpdateUserDto("Nuevo Nombre"),
                typeof(UserNotFoundException)
            };
        }

        // =================================================================================================
        // 3. DELETE USER
        // =================================================================================================

        [Theory]
        [MemberData(nameof(GetDeleteUserSuccessScenarios))]
        public async Task Handle_DeleteUser_Success(string scenarioName, Guid targetId, User userFoundInDb)
        {
            // ARRANGE
            _userRepositoryMock.Setup(x => x.GetByIdAsync(targetId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userFoundInDb);

            var handler = new DeleteUserCommandHandler(_userRepositoryMock.Object, _keycloakServiceMock.Object, _auditServiceMock.Object);
            var command = new DeleteUserCommand(targetId);

            // ACT
            await handler.Handle(command, CancellationToken.None);

            // ASSERT
            _keycloakServiceMock.Verify(x => x.DeactivateUserAsync(userFoundInDb.KeycloakId, It.IsAny<CancellationToken>()), Times.Once);
            userFoundInDb.State.Should().Be(UserState.Inactive);
            _userRepositoryMock.Verify(x => x.AddHistoryAsync(It.IsAny<UserHistory>(), It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(x => x.LogAsync(It.Is<AuditLog>(l => l.Action == "DeleteUser")), Times.Once);
        }

        [Theory]
        [MemberData(nameof(GetDeleteUserFailureScenarios))]
        public async Task Handle_DeleteUser_Failure(string scenarioName, Guid targetId, Type expectedExceptionType)
        {
            // ARRANGE
            _userRepositoryMock.Setup(x => x.GetByIdAsync(targetId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            var handler = new DeleteUserCommandHandler(_userRepositoryMock.Object, _keycloakServiceMock.Object, _auditServiceMock.Object);
            var command = new DeleteUserCommand(targetId);

            // ACT
            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            // ASSERT
            await act.Should().ThrowAsync<Exception>()
               .Where(e => e.GetType() == expectedExceptionType);
        }

        public static IEnumerable<object?[]> GetDeleteUserSuccessScenarios()
        {
            var validId = Guid.NewGuid();
            var existingUser = new User("To Delete", "del@test.com", "kc-del", UserRole.User);

            yield return new object?[] { "Success_SoftDelete", validId, existingUser };
        }

        public static IEnumerable<object?[]> GetDeleteUserFailureScenarios()
        {
            yield return new object?[] { "Fail_NotFound", Guid.NewGuid(), typeof(UserNotFoundException) };
        }
    }
}