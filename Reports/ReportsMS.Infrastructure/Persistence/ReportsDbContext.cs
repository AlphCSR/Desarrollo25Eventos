using Microsoft.EntityFrameworkCore;
using ReportsMS.Domain.Entities;
using ReportsMS.Domain.ValueObjects;

namespace ReportsMS.Infrastructure.Persistence
{
    public class ReportsDbContext : DbContext
    {
        public DbSet<SalesRecord> SalesRecords { get; set; }
        public DbSet<EventStats> EventStats { get; set; }
        public DbSet<DashboardMetric> DashboardMetrics { get; set; }

        public ReportsDbContext(DbContextOptions<ReportsDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SalesRecord>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount)
                    .HasConversion(v => v.Amount, v => Money.Create(v, "USD"))
                    .HasColumnType("decimal(18,2)");
                
                entity.Property(e => e.UserEmail)
                    .HasConversion(v => v.Value, v => Email.Create(v))
                    .HasMaxLength(150);

                entity.HasIndex(e => e.EventId); 
            });
            
            modelBuilder.Entity<EventStats>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.EventId).IsUnique();
            });

            modelBuilder.Entity<DashboardMetric>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.MetricName).IsUnique();
                entity.Property(e => e.Value).HasColumnType("decimal(18,2)");
            });
            
            base.OnModelCreating(modelBuilder);
        }
    }
}
