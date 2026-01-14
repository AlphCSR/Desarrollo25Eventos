using Microsoft.EntityFrameworkCore;
using BookingMS.Domain.Entities;
using BookingMS.Infrastructure.Persistence.Configuration;

namespace BookingMS.Infrastructure.Persistence.Configuration
{
    public class BookingDbContext : DbContext
    {
        public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options) { }

        public DbSet<Booking> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new BookingConfiguration());
        }
    }
}