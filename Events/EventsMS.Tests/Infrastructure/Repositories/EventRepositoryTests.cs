using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using EventsMS.Domain.Entities;
using EventsMS.Infrastructure.Persistence;
using EventsMS.Infrastructure.Repository;
using EventsMS.Shared.Enums;
using Xunit;
using System.Linq;

namespace EventsMS.Tests.Infrastructure.Repositories
{
    public class EventRepositoryTests
    {
        private readonly DbContextOptions<EventsDbContext> _dbOptions;

        public EventRepositoryTests()
        {
            _dbOptions = new DbContextOptionsBuilder<EventsDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB per test
                .Options;
        }

        [Fact]
        public async Task AddAsync_ShouldAddEventToDatabase()
        {
            // Arrange
            using var context = new EventsDbContext(_dbOptions);
            var repository = new EventRepository(context);
            var evt = new Event("Test Event", "Desc", DateTime.UtcNow.AddDays(1), "Venue", "Category");

            // Act
            await repository.AddAsync(evt, System.Threading.CancellationToken.None);
            await repository.SaveChangesAsync(System.Threading.CancellationToken.None);

            // Assert
            using var assertContext = new EventsDbContext(_dbOptions);
            var savedEvent = await assertContext.Events.FirstOrDefaultAsync(e => e.Title == "Test Event");
            savedEvent.Should().NotBeNull();
            savedEvent!.Category.Should().Be("Category");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnEvent_WhenExists()
        {
            // Arrange
            var evt = new Event("Existing Event", "Desc", DateTime.UtcNow.AddDays(1), "Venue", "Category");
            using (var context = new EventsDbContext(_dbOptions))
            {
                context.Events.Add(evt);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new EventsDbContext(_dbOptions))
            {
                var repository = new EventRepository(context);
                var result = await repository.GetByIdAsync(evt.Id, System.Threading.CancellationToken.None);

                // Assert
                result.Should().NotBeNull();
                result!.Id.Should().Be(evt.Id);
            }
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateEvent()
        {
            // Arrange
            var evt = new Event("To Update", "Desc", DateTime.UtcNow.AddDays(1), "Venue", "Category");
            using (var context = new EventsDbContext(_dbOptions))
            {
                context.Events.Add(evt);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new EventsDbContext(_dbOptions))
            {
                var repository = new EventRepository(context);
                var eventToUpdate = await repository.GetByIdAsync(evt.Id, System.Threading.CancellationToken.None);
                
                eventToUpdate!.UpdateDetails("Updated Title", "Updated Desc", DateTime.UtcNow.AddDays(2), "Updated Venue", "Updated Cat");
                
                await repository.UpdateAsync(eventToUpdate, System.Threading.CancellationToken.None);
                await repository.SaveChangesAsync(System.Threading.CancellationToken.None);
            }

            // Assert
            using (var context = new EventsDbContext(_dbOptions))
            {
                var updatedEvent = await context.Events.FindAsync(evt.Id);
                updatedEvent!.Title.Should().Be("Updated Title");
                updatedEvent.Category.Should().Be("Updated Cat");
            }
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllEvents()
        {
            // Arrange
            using (var context = new EventsDbContext(_dbOptions))
            {
                context.Events.Add(new Event("Event 1", "Desc", DateTime.UtcNow.AddDays(1), "Venue", "Cat1"));
                context.Events.Add(new Event("Event 2", "Desc", DateTime.UtcNow.AddDays(2), "Venue", "Cat2"));
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new EventsDbContext(_dbOptions))
            {
                var repository = new EventRepository(context);
                var result = await repository.GetAllAsync(System.Threading.CancellationToken.None);

                // Assert
                result.Should().HaveCount(2);
            }
        }
    }
}
