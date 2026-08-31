using Microsoft.EntityFrameworkCore;
using ScreenTime.Domain.Entities;

namespace ScreenTime.Infrastructure.Data;

/// <summary>
/// Development seed data. Idempotent - it does nothing if the database already
/// holds films, so it is safe to run on every start-up.
/// </summary>
public static class SeedData
{
    private const int CleaningMinutes = 20;

    // Four hours apart, which is wider than the longest film plus cleaning,
    // so no two screenings in the same auditorium can overlap.
    private static readonly int[] StartHours = [12, 16, 20];

    public static async Task SeedAsync(ScreenTimeDbContext db)
    {
        if (await db.Movies.AnyAsync())
        {
            return;
        }

        var standard = new SeatType { Name = "Standard", Price = 120m };
        var vip = new SeatType { Name = "VIP", Price = 180m };
        db.SeatTypes.AddRange(standard, vip);

        var genres = new[] { "Action", "Animerat", "Drama", "Komedi", "Sci-Fi", "Skräck", "Thriller" }
            .ToDictionary(name => name, name => new Genre { Name = name });
        db.Genres.AddRange(genres.Values);

        var movies = new[]
        {
            new Movie
            {
                Title = "Dune: Part Two",
                AgeRating = 11,
                RuntimeMinutes = 166,
                Description = "Paul Atreides förenar sig med fremen för att hämnas dem som förgjorde hans familj."
            },
            new Movie
            {
                Title = "Oppenheimer",
                AgeRating = 15,
                RuntimeMinutes = 180,
                Description = "Fysikern J. Robert Oppenheimer leder arbetet med att bygga den första atombomben."
            },
            new Movie
            {
                Title = "Barbie",
                AgeRating = 7,
                RuntimeMinutes = 114,
                Description = "Barbie lämnar Barbieland för den verkliga världen och får en existentiell kris på köpet."
            },
            new Movie
            {
                Title = "Spider-Man: Across the Spider-Verse",
                AgeRating = 11,
                RuntimeMinutes = 140,
                Description = "Miles Morales kastas genom multiversumet och möter ett helt sällskap av spindlar."
            },
            new Movie
            {
                Title = "Poor Things",
                AgeRating = 15,
                RuntimeMinutes = 141,
                Description = "Bella Baxter återuppväcks av en excentrisk vetenskapsman och ger sig ut i världen."
            },
            new Movie
            {
                Title = "Wicked",
                AgeRating = 7,
                RuntimeMinutes = 160,
                Description = "Vänskapen mellan Elphaba och Glinda innan de blir den onda och den goda häxan."
            }
        };
        db.Movies.AddRange(movies);

        db.MovieGenres.AddRange(Link(movies[0], "Sci-Fi", "Action"));
        db.MovieGenres.AddRange(Link(movies[1], "Drama", "Thriller"));
        db.MovieGenres.AddRange(Link(movies[2], "Komedi"));
        db.MovieGenres.AddRange(Link(movies[3], "Animerat", "Action"));
        db.MovieGenres.AddRange(Link(movies[4], "Drama", "Komedi"));
        db.MovieGenres.AddRange(Link(movies[5], "Drama"));

        var stora = new Auditorium { Name = "Stora salongen", IsActive = true };
        var lilla = new Auditorium { Name = "Lilla salongen", IsActive = true };
        db.Auditoriums.AddRange(stora, lilla);

        // Stora salongen: 8 rows of 12, the back two rows are VIP. 96 seats.
        db.Seats.AddRange(BuildSeats(stora, "ABCDEFGH", seatsPerRow: 12, vipRows: "GH"));
        // Lilla salongen: 5 rows of 8, all standard. 40 seats.
        db.Seats.AddRange(BuildSeats(lilla, "ABCDE", seatsPerRow: 8, vipRows: ""));

        // Two weeks of programme starting tomorrow.
        var firstDay = DateTime.Today.AddDays(1);
        for (var day = 0; day < 14; day++)
        {
            for (var i = 0; i < movies.Length; i++)
            {
                var movie = movies[i];
                var start = firstDay.AddDays(day).AddHours(StartHours[(day + i) % StartHours.Length]);

                db.Screenings.Add(new Screening
                {
                    Movie = movie,
                    Auditorium = i % 2 == 0 ? stora : lilla,
                    StartTime = start,
                    EndTime = start.AddMinutes(movie.RuntimeMinutes + CleaningMinutes),
                    IsCancelled = false
                });
            }
        }

        await db.SaveChangesAsync();

        IEnumerable<MovieGenre> Link(Movie movie, params string[] names) =>
            names.Select(name => new MovieGenre { Movie = movie, Genre = genres[name] });

        List<Seat> BuildSeats(Auditorium auditorium, string rows, int seatsPerRow, string vipRows) =>
        [
            .. from row in rows
               from number in Enumerable.Range(1, seatsPerRow)
               select new Seat
               {
                   Auditorium = auditorium,
                   RowLabel = row.ToString(),
                   SeatNumber = number,
                   SeatType = vipRows.Contains(row) ? vip : standard
               }
        ];
    }
}
