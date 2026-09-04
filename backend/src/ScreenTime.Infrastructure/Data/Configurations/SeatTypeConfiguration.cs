using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScreenTime.Domain.Entities;

namespace ScreenTime.Infrastructure.Data.Configurations;

public class SeatTypeConfiguration : IEntityTypeConfiguration<SeatType>
{
    public void Configure(EntityTypeBuilder<SeatType> builder)
    {
        builder.Property(st => st.Name).IsRequired().HasMaxLength(50);
        builder.HasIndex(st => st.Name).IsUnique();

        // Price lives here, not on Seat: one price per seat type.
        builder.Property(st => st.Price).HasPrecision(18, 2);
    }
}
