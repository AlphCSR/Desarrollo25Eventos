using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SeatingMS.Domain.Entities;
using SeatingMS.Infrastructure.Persistence;
using SeatingMS.Infrastructure.Repositories;
using Xunit;

namespace SeatingMS.Tests.Infrastructure.Repositories
{
    public class EventSeatRepositoryTests
    {
        private readonly DbContextOptions<SeatingDbContext> _dbOptions;

        public EventSeatRepositoryTests()
        {
            _dbOptions = new DbContextOptionsBuilder<SeatingDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task AddRangeAsync_ShouldAddSeats()
        {
            // Arrange
            var seats = new List<EventSeat>
            {
                new EventSeat(Guid.NewGuid(), Guid.NewGuid(), "A", 1),
                new EventSeat(Guid.NewGuid(), Guid.NewGuid(), "A", 2)
            };

            using var context = new SeatingDbContext(_dbOptions);
            var repository = new EventSeatRepository(context);

            // Act
            await repository.AddRangeAsync(seats, CancellationToken.None);
            await repository.SaveChangesAsync(CancellationToken.None);

            // Assert
            using var assertContext = new SeatingDbContext(_dbOptions);
            assertContext.EventSeats.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnSeat_WhenExists()
        {
            // Arrange
            var seat = new EventSeat(Guid.NewGuid(), Guid.NewGuid(), "B", 1);
            // Reflection to set ID
            var seatId = Guid.NewGuid();
            typeof(EventSeat).GetProperty("Id")?.SetValue(seat, seatId);

            using (var context = new SeatingDbContext(_dbOptions))
            {
                context.EventSeats.Add(seat);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new SeatingDbContext(_dbOptions))
            {
                var repository = new EventSeatRepository(context);
                var result = await repository.GetByIdAsync(seatId, CancellationToken.None);

                // Assert
                result.Should().NotBeNull();
                result!.Row.Should().Be("B");
            }
        }

        [Fact]
        public async Task GetByEventIdAsync_ShouldReturnSeatsForEvent()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var otherEventId = Guid.NewGuid();
            var seats = new List<EventSeat>
            {
                new EventSeat(eventId, Guid.NewGuid(), "C", 1),
                new EventSeat(eventId, Guid.NewGuid(), "C", 2),
                new EventSeat(otherEventId, Guid.NewGuid(), "D", 1)
            };

            using (var context = new SeatingDbContext(_dbOptions))
            {
                context.EventSeats.AddRange(seats);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new SeatingDbContext(_dbOptions))
            {
                var repository = new EventSeatRepository(context);
                var result = await repository.GetByEventIdAsync(eventId, CancellationToken.None);

                // Assert
                result.Should().HaveCount(2);
                result.All(s => s.EventId == eventId).Should().BeTrue();
            }
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateSeat()
        {
            // Arrange
            var seat = new EventSeat(Guid.NewGuid(), Guid.NewGuid(), "E", 1);
            var seatId = Guid.NewGuid();
            typeof(EventSeat).GetProperty("Id")?.SetValue(seat, seatId);

            using (var context = new SeatingDbContext(_dbOptions))
            {
                context.EventSeats.Add(seat);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new SeatingDbContext(_dbOptions))
            {
                var repository = new EventSeatRepository(context);
                var seatToUpdate = await repository.GetByIdAsync(seatId, CancellationToken.None);
                seatToUpdate!.Lock(Guid.NewGuid());
                await repository.UpdateAsync(seatToUpdate, CancellationToken.None);
                await repository.SaveChangesAsync(CancellationToken.None);
            }

            // Assert
            using (var context = new SeatingDbContext(_dbOptions))
            {
                var updatedSeat = await context.EventSeats.FindAsync(seatId);
                updatedSeat!.Status.Should().Be(Shared.Enum.SeatStatus.Locked);
            }
        }
    }
}
