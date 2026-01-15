using Microsoft.EntityFrameworkCore;
using PaymentsMS.Domain.Entities;
using PaymentsMS.Domain.ValueObjects;

namespace PaymentsMS.Infrastructure.Persistence
{
    public class PaymentsDbContext : DbContext
    {
        public DbSet<Payment> Payments { get; set; }

        public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.OwnsOne(e => e.Amount, money =>
                {
                    money.Property(m => m.Amount).HasColumnName("Amount").HasColumnType("decimal(18,2)");
                    money.Property(m => m.Currency).HasColumnName("Currency").HasMaxLength(3);
                });

                entity.Property(e => e.Email)
                    .HasConversion(v => v.Value, v => Email.Create(v))
                    .HasMaxLength(150);

                entity.Property(e => e.StripePaymentIntentId).IsRequired(false);
            });
            
            base.OnModelCreating(modelBuilder);
        }
    }
}
