using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EventsMS.Domain.Entities;

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

            // Configuración de la relación 1:N con EventSection
            builder.HasMany(e => e.Sections)
                .WithOne()
                .HasForeignKey(s => s.EventId)
                .OnDelete(DeleteBehavior.Cascade); // Si borro evento, se borran secciones
        }
    }

    public class EventSectionConfiguration : IEntityTypeConfiguration<EventSection>
    {
        public void Configure(EntityTypeBuilder<EventSection> builder)
        {
            builder.ToTable("EventSections");
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Name).IsRequired();
            builder.Property(s => s.Price).HasPrecision(18, 2);

            // Configuración de la relación 1:N con Seat
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
            
            // Índice para búsquedas rápidas por estado o código
            builder.HasIndex(s => s.Status);
        }
    }
}