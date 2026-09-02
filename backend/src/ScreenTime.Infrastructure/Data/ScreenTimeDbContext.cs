using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ScreenTime.Domain.Entities;
using ScreenTime.Infrastructure.Identity;

namespace ScreenTime.Infrastructure.Data;

/// <summary>
/// Database context for ScreenTime application with ASP.NET Identity support.
/// Inherits from IdentityDbContext to include Identity tables for users and roles.
/// </summary>
public class ScreenTimeDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public ScreenTimeDbContext(DbContextOptions<ScreenTimeDbContext> options)
        : base(options)
    {
    }

    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<MovieGenre> MovieGenres => Set<MovieGenre>();
    public DbSet<Auditorium> Auditoriums => Set<Auditorium>();
    public DbSet<SeatType> SeatTypes => Set<SeatType>();
    public DbSet<Seat> Seats => Set<Seat>();
    public DbSet<Screening> Screenings => Set<Screening>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Ticket> Tickets => Set<Ticket>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ScreenTimeDbContext).Assembly);
    }
}
