using Microsoft.EntityFrameworkCore;
using MarketingMS.Domain.Entities;
using MarketingMS.Domain.ValueObjects;

namespace MarketingMS.Infrastructure.Persistence
{
    public class MarketingDbContext : DbContext
    {
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<UserInterest> UserInterests { get; set; }

        public MarketingDbContext(DbContextOptions<MarketingDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Coupon>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Code).IsUnique();
                entity.Property(e => e.Code)
                    .HasConversion(v => v.Value, v => CouponCode.Create(v))
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(e => e.Value).HasColumnType("decimal(18,2)");
                entity.Property(e => e.MinimumAmount).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<UserInterest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.UserId, e.Category }).IsUnique();
                entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
