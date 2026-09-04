using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScreenTime.Domain.Entities;

namespace ScreenTime.Infrastructure.Data.Configurations;

public class MovieGenreConfiguration : IEntityTypeConfiguration<MovieGenre>
{
    public void Configure(EntityTypeBuilder<MovieGenre> builder)
    {
        builder.HasKey(mg => new { mg.MovieId, mg.GenreId });

        builder.HasOne(mg => mg.Movie)
            .WithMany()
            .HasForeignKey(mg => mg.MovieId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(mg => mg.Genre)
            .WithMany()
            .HasForeignKey(mg => mg.GenreId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}