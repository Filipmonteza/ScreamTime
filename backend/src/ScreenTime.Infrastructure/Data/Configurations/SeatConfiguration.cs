using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScreenTime.Domain.Entities;

namespace ScreenTime.Infrastructure.Data.Configurations;

public class SeatConfiguration : IEntityTypeConfiguration<Seat>
{
    public void Configure(EntityTypeBuilder<Seat> builder)
    {
        builder.Property(s => s.RowLabel).IsRequired().HasMaxLength(5);

        // No two seats share a row and number inside the same auditorium.
        builder.HasIndex(s => new { s.AuditoriumId, s.RowLabel, s.SeatNumber }).IsUnique();

        builder.HasOne(s => s.Auditorium)
            .WithMany()
            .HasForeignKey(s => s.AuditoriumId)
            .OnDelete(DeleteBehavior.Cascade);

        // A seat type in use must not be deletable.
        builder.HasOne(s => s.SeatType)
            .WithMany()
            .HasForeignKey(s => s.SeatTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
