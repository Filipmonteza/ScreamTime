using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScreenTime.Domain.Entities;

namespace ScreenTime.Infrastructure.Data.Configurations;

public class ScreeningConfiguration : IEntityTypeConfiguration<Screening>
{
    public void Configure(EntityTypeBuilder<Screening> builder)
    {
        builder.HasOne(s => s.Movie)
            .WithMany()
            .HasForeignKey(s => s.MovieId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Auditorium)
            .WithMany()
            .HasForeignKey(s => s.AuditoriumId)
            .OnDelete(DeleteBehavior.Restrict);

        // Supports "what is on in this auditorium" and the overlap check that
        // has to be done in application code.
        builder.HasIndex(s => new { s.AuditoriumId, s.StartTime });
    }
}
