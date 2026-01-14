using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EventsMS.Domain.Entities;
using EventsMS.Domain.ValueObjects;

namespace EventsMS.Infrastructure.Persistence.Configuration
{
    public class EventConfiguration : IEntityTypeConfiguration<Event>
    {
        public void Configure(EntityTypeBuilder<Event> builder)
        {
            builder.ToTable("Events");
            
            builder.HasKey(e => e.Id);
            
            builder.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.Description)
                .HasMaxLength(1000);

            builder.OwnsOne(e => e.DateRange, dr =>
            {
                dr.Property(p => p.StartDate).HasColumnName("Date").IsRequired();
                dr.Property(p => p.EndDate).HasColumnName("EndDate").IsRequired();
            });

            builder.PrimitiveCollection(e => e.Categories);

            builder.HasMany(e => e.Sections)
                .WithOne()
                .HasForeignKey(s => s.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class EventSectionConfiguration : IEntityTypeConfiguration<EventSection>
    {
        public void Configure(EntityTypeBuilder<EventSection> builder)
        {
            builder.ToTable("EventSections");
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Name).IsRequired();
            builder.Property(s => s.Price)
                .HasConversion(v => v.Amount, v => Money.Create(v, "USD"))
                .HasPrecision(18, 2);

            builder.HasMany(s => s.Seats)
                .WithOne()
                .HasForeignKey(st => st.SectionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class SeatConfiguration : IEntityTypeConfiguration<Seat>
    {
        public void Configure(EntityTypeBuilder<Seat> builder)
        {
            builder.ToTable("Seats");
            builder.HasKey(s => s.Id);
            
            builder.HasIndex(s => s.Status);
        }
    }
}