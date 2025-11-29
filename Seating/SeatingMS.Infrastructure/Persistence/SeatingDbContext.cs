using Microsoft.EntityFrameworkCore;
using SeatingMS.Domain.Entities;
using SeatingMS.Infrastructure.Persistence.Configuration;

using System.Diagnostics.CodeAnalysis;

namespace SeatingMS.Infrastructure.Persistence
{
    [ExcludeFromCodeCoverage]
    public class SeatingDbContext : DbContext
    {
        public SeatingDbContext(DbContextOptions<SeatingDbContext> options) : base(options) { }

        public DbSet<EventSeat> EventSeats { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new EventSeatConfiguration());
        }
    }
}