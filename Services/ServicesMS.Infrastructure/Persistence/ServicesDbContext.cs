using Microsoft.EntityFrameworkCore;
using ServicesMS.Domain.Entities;
using ServicesMS.Domain.ValueObjects;

namespace ServicesMS.Infrastructure.Persistence
{
    public class ServicesDbContext : DbContext
    {
        public DbSet<ServiceDefinition> ServiceDefinitions { get; set; }
        public DbSet<ServiceBooking> ServiceBookings { get; set; }

        public ServicesDbContext(DbContextOptions<ServicesDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ServiceDefinition>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.BasePrice)
                    .HasConversion(v => v.Amount, v => Money.Create(v, "USD"))
                    .HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<ServiceBooking>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TotalPrice)
                    .HasConversion(v => v.Amount, v => Money.Create(v, "USD"))
                    .HasColumnType("decimal(18,2)");
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
