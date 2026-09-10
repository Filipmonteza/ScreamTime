using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScreenTime.Domain.Entities;

namespace ScreenTime.Infrastructure.Data.Configurations;

public class AuditoriumConfiguration : IEntityTypeConfiguration<Auditorium>
{
    public void Configure(EntityTypeBuilder<Auditorium> builder)
    {
        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(50)
            .UseCollation("NOCASE");

        builder.HasIndex(a => a.Name)
            .IsUnique()
            .HasFilter("[IsActive] = 1");
    }
}
