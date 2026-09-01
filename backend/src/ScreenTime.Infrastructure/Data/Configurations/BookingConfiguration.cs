using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScreenTime.Domain.Entities;

namespace ScreenTime.Infrastructure.Data.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        // Configure foreign key relationship to ApplicationUser
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(b => b.UserId);

        builder.Property(b => b.BookingReference)
            .IsRequired()
            .HasMaxLength(12);

        builder.HasIndex(b => b.BookingReference)
            .IsUnique();

        builder.Property(b => b.TotalAmount)
            .HasPrecision(18, 2);

        builder.HasOne(b => b.Screening)
            .WithMany(s => s.Bookings)
            .HasForeignKey(b => b.ScreeningId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
