using Microsoft.EntityFrameworkCore;
using EventsMS.Domain.Entities;
using EventsMS.Infrastructure.Persistence.Configuration;

using System.Diagnostics.CodeAnalysis;

namespace EventsMS.Infrastructure.Persistence
{
    [ExcludeFromCodeCoverage]
    public class EventsDbContext : DbContext
    {
        public EventsDbContext(DbContextOptions<EventsDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            base.OnConfiguring(optionsBuilder);
        }

        public DbSet<Event> Events { get; set; }
        public DbSet<EventSection> EventSections { get; set; }
        public DbSet<Seat> Seats { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(EventsDbContext).Assembly);
        }
    }
}
