using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BookingMS.Domain.Entities;

namespace BookingMS.Infrastructure.Persistence.Configuration
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.ToTable("Bookings");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.TotalAmount).HasPrecision(18, 2);
            builder.Property(x => x.Status).HasConversion<string>();
            builder.Property("_seatIds")
                .HasColumnName("SeatIds")
                .HasColumnType("jsonb");
        }
    }
}