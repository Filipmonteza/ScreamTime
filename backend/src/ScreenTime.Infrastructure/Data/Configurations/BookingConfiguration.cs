using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScreenTime.Domain.Entities;

namespace ScreenTime.Infrastructure.Data.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        // No FK yet - ApplicationUser does not exist until Identity is added.
        // It will inherit IdentityUser<Guid>, so this stays a Guid and can be
        // turned into a real foreign key then.
        builder.HasIndex(b => b.UserId);

        builder.Property(b => b.BookingReference).IsRequired().HasMaxLength(12);
        builder.HasIndex(b => b.BookingReference).IsUnique();

        builder.Property(b => b.TotalAmount).HasPrecision(18, 2);

        builder.HasOne(b => b.Screening)
            .WithMany(s => s.Bookings)
            .HasForeignKey(b => b.ScreeningId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
