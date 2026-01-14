using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SeatingMS.Domain.Entities;

using System.Diagnostics.CodeAnalysis;

namespace SeatingMS.Infrastructure.Persistence.Configuration
{
    [ExcludeFromCodeCoverage]
    public class EventSeatConfiguration : IEntityTypeConfiguration<EventSeat>
    {
        public void Configure(EntityTypeBuilder<EventSeat> builder)
        {
            builder.ToTable("EventSeats");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Row).IsRequired().HasMaxLength(10);
            builder.Property(x => x.Status).IsRequired();
            
            builder.HasIndex(x => new { x.EventId, x.SectionId, x.Row, x.Number }).IsUnique();
            
            builder.Property(x => x.Status).IsConcurrencyToken();
        }
    }
}