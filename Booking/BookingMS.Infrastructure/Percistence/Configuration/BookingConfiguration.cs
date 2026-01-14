using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BookingMS.Domain.Entities;
using BookingMS.Domain.ValueObjects;

namespace BookingMS.Infrastructure.Persistence.Configuration
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.ToTable("Bookings");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.TotalAmount)
                .HasConversion(v => v.Amount, v => Money.Create(v, "USD"))
                .HasPrecision(18, 2);

            builder.Property(x => x.DiscountAmount)
                .HasConversion(v => v.Amount, v => Money.Create(v, "USD"))
                .HasPrecision(18, 2);

            builder.Property(x => x.Email)
                .HasConversion(v => v.Value, v => Email.Create(v))
                .HasMaxLength(150);

            builder.Property(x => x.Status).HasConversion<string>();
            builder.Property(x => x.PaymentReminderSent).HasDefaultValue(false);
            builder.Property("_seatIds")
                .HasColumnName("SeatIds")
                .HasColumnType("jsonb");

            builder.Property("_serviceIds")
                .HasColumnName("ServiceIds")
                .HasColumnType("jsonb");
        }
    }
}