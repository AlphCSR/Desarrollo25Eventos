using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BookingMS.Domain.Entities;
using BookingMS.Infrastructure.Persistence.Configuration;
using BookingMS.Infrastructure.Repositories;
using BookingMS.Shared.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BookingMS.Tests.Infrastructure.Repositories
{
    public class BookingRepositoryTests
    {
        private readonly DbContextOptions<BookingDbContext> _dbOptions;

        public BookingRepositoryTests()
        {
            _dbOptions = new DbContextOptionsBuilder<BookingDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task AddAsync_ShouldAddBooking()
        {
            // Arrange
            var booking = new Booking(Guid.NewGuid(), Guid.NewGuid(), new List<Guid>(), 100);
            using var context = new BookingDbContext(_dbOptions);
            var repository = new BookingRepository(context);

            // Act
            await repository.AddAsync(booking, CancellationToken.None);
            await repository.SaveChangesAsync(CancellationToken.None);

            // Assert
            using var assertContext = new BookingDbContext(_dbOptions);
            assertContext.Bookings.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnBooking_WhenExists()
        {
            // Arrange
            var booking = new Booking(Guid.NewGuid(), Guid.NewGuid(), new List<Guid>(), 100);
            var bookingId = Guid.NewGuid();
            typeof(Booking).GetProperty("Id")?.SetValue(booking, bookingId);

            using (var context = new BookingDbContext(_dbOptions))
            {
                context.Bookings.Add(booking);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new BookingDbContext(_dbOptions))
            {
                var repository = new BookingRepository(context);
                var result = await repository.GetByIdAsync(bookingId, CancellationToken.None);

                // Assert
                result.Should().NotBeNull();
                result!.Id.Should().Be(bookingId);
            }
        }

        [Fact]
        public async Task GetActiveByEventAsync_ShouldReturnActiveBooking()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var eventId = Guid.NewGuid();
            var booking = new Booking(userId, eventId, new List<Guid>(), 100);
            
            using (var context = new BookingDbContext(_dbOptions))
            {
                context.Bookings.Add(booking);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new BookingDbContext(_dbOptions))
            {
                var repository = new BookingRepository(context);
                var result = await repository.GetActiveByEventAsync(userId, eventId, CancellationToken.None);

                // Assert
                result.Should().NotBeNull();
                result!.Status.Should().Be(BookingStatus.PendingPayment);
            }
        }

        [Fact]
        public async Task GetByUserIdAsync_ShouldReturnBookingsForUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var bookings = new List<Booking>
            {
                new Booking(userId, Guid.NewGuid(), new List<Guid>(), 100),
                new Booking(userId, Guid.NewGuid(), new List<Guid>(), 200),
                new Booking(otherUserId, Guid.NewGuid(), new List<Guid>(), 300)
            };

            using (var context = new BookingDbContext(_dbOptions))
            {
                context.Bookings.AddRange(bookings);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new BookingDbContext(_dbOptions))
            {
                var repository = new BookingRepository(context);
                var result = await repository.GetByUserIdAsync(userId);

                // Assert
                result.Should().HaveCount(2);
                result.All(b => b.UserId == userId).Should().BeTrue();
            }
        }
    }
}
