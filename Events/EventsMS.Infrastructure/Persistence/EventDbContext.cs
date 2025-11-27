using Microsoft.EntityFrameworkCore;
using EventsMS.Domain.Entities;
using EventsMS.Infrastructure.Persistence.Configuration;

namespace EventsMS.Infrastructure.Persistence
{
    public class EventsDbContext : DbContext
    {
        public EventsDbContext(DbContextOptions<EventsDbContext> options) : base(options)
        {
        }

        public DbSet<Event> Events { get; set; }
        public DbSet<EventSection> EventSections { get; set; }
        public DbSet<Seat> Seats { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Aplicar las configuraciones definidas arriba
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(EventsDbContext).Assembly);
        }
    }
}