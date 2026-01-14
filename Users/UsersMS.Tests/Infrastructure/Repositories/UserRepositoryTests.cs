using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using UsersMS.Domain.Entities;
using UsersMS.Infrastructure.Persistence;
using UsersMS.Infrastructure.Repositories;
using UsersMS.Shared.Enums;
using Xunit;

namespace UsersMS.Tests.Infrastructure.Repositories
{
    public class UserRepositoryTests
    {
        private readonly DbContextOptions<UsersDbContext> _dbOptions;

        public UserRepositoryTests()
        {
            _dbOptions = new DbContextOptionsBuilder<UsersDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB per test
                .Options;
        }

        [Fact]
        public async Task AddAsync_ShouldAddUserToDatabase()
        {
            // Arrange
            using var context = new UsersDbContext(_dbOptions);
            var repository = new UserRepository(context);
            var user = new User("Test User", "test@example.com", "kc-123", UserRole.User);

            // Act
            await repository.AddAsync(user);
            await repository.SaveChangesAsync();

            // Assert
            using var assertContext = new UsersDbContext(_dbOptions);
            var savedUser = await assertContext.Users.FirstOrDefaultAsync(u => u.Email == "test@example.com");
            savedUser.Should().NotBeNull();
            savedUser!.FullName.Should().Be("Test User");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var user = new User("Existing User", "exist@example.com", "kc-456", UserRole.Admin);
            using (var context = new UsersDbContext(_dbOptions))
            {
                context.Users.Add(user);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new UsersDbContext(_dbOptions))
            {
                var repository = new UserRepository(context);
                var result = await repository.GetByIdAsync(user.Id);

                // Assert
                result.Should().NotBeNull();
                result!.Id.Should().Be(user.Id);
            }
        }

        [Fact]
        public async Task GetByEmailAsync_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var email = "email@example.com";
            var user = new User("Email User", email, "kc-789", UserRole.User);
            using (var context = new UsersDbContext(_dbOptions))
            {
                context.Users.Add(user);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new UsersDbContext(_dbOptions))
            {
                var repository = new UserRepository(context);
                var result = await repository.GetByEmailAsync(email);

                // Assert
                result.Should().NotBeNull();
                result!.Email.Should().Be(email);
            }
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateUser()
        {
            // Arrange
            var user = new User("To Update", "update@example.com", "kc-update", UserRole.User);
            using (var context = new UsersDbContext(_dbOptions))
            {
                context.Users.Add(user);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new UsersDbContext(_dbOptions))
            {
                var repository = new UserRepository(context);
                var userToUpdate = await repository.GetByIdAsync(user.Id);
                userToUpdate!.UpdateProfile("Updated Name");
                await repository.UpdateAsync(userToUpdate);
                await repository.SaveChangesAsync();
            }

            // Assert
            using (var context = new UsersDbContext(_dbOptions))
            {
                var updatedUser = await context.Users.FindAsync(user.Id);
                updatedUser!.FullName.Should().Be("Updated Name");
            }
        }

        [Fact]
        public async Task AddHistoryAsync_ShouldAddHistoryToDatabase()
        {
            // Arrange
            var user = new User("History User", "history@example.com", "kc-hist", UserRole.User);
            using (var context = new UsersDbContext(_dbOptions))
            {
                context.Users.Add(user);
                await context.SaveChangesAsync();
            }

            var history = new UserHistory(user.Id, "Action", "Details");

            // Act
            using (var context = new UsersDbContext(_dbOptions))
            {
                var repository = new UserRepository(context);
                await repository.AddHistoryAsync(history);
                await repository.SaveChangesAsync();
            }

            // Assert
            using (var context = new UsersDbContext(_dbOptions))
            {
                var savedHistory = await context.Set<UserHistory>().FirstOrDefaultAsync(h => h.UserId == user.Id && h.Action == "Action");
                savedHistory.Should().NotBeNull();
                savedHistory!.Action.Should().Be("Action");
            }
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllUsers()
        {
            // Arrange
            using (var context = new UsersDbContext(_dbOptions))
            {
                context.Users.Add(new User("User 1", "u1@example.com", "kc-1", UserRole.User));
                context.Users.Add(new User("User 2", "u2@example.com", "kc-2", UserRole.User));
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new UsersDbContext(_dbOptions))
            {
                var repository = new UserRepository(context);
                var result = await repository.GetAllAsync();

                // Assert
                result.Should().HaveCount(2);
            }
        }
    }
}
