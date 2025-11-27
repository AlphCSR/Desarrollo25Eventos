using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UsersMS.Domain.Entities;

namespace UsersMS.Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(150);
            
            builder.HasIndex(u => u.Email).IsUnique();

            builder.Property(u => u.KeycloakId)
                .IsRequired();

            builder.Property(u => u.Preferences)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.List<string>>(v, (System.Text.Json.JsonSerializerOptions)null) ?? new System.Collections.Generic.List<string>()
                )
                .IsRequired(false);
            
            builder.HasMany(u => u.History)
                .WithOne()
                .HasForeignKey(h => h.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        public void Configure(EntityTypeBuilder<UserHistory> builder)
        {
            builder.ToTable("UserHistories");
            builder.HasKey(h => h.Id);
            builder.Property(h => h.Id).ValueGeneratedNever();
            builder.HasIndex(h => h.UserId);  
        }
    }
}