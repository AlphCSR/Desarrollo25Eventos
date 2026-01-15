using Microsoft.EntityFrameworkCore;
using SurveyMS.Domain.Entities;
using SurveyMS.Domain.ValueObjects;

namespace SurveyMS.Infrastructure.Persistence
{
    public class SurveyDbContext : DbContext
    {
        public SurveyDbContext(DbContextOptions<SurveyDbContext> options) : base(options) { }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            base.OnConfiguring(optionsBuilder);
        }

        public DbSet<Feedback> Feedbacks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Feedback>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Rating)
                    .HasConversion(v => v.Value, v => Rating.Create(v));
                entity.Property(e => e.Comment).HasMaxLength(1000);
                entity.HasIndex(e => new { e.UserId, e.BookingId }).IsUnique(); 
            });
        }
    }
}
