using Microsoft.EntityFrameworkCore;
using ScreenTime.Domain.Entities;

namespace ScreenTime.Infrastructure.Data;

public class ScreenTimeDbContext : DbContext
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
