using Microsoft.EntityFrameworkCore;
using ScreenTime.Domain.Common;
using ScreenTime.Domain.Entities;
using ScreenTime.Domain.Enums;

namespace ScreenTime.Infrastructure.Data;

/// <summary>
/// Development seed data. Idempotent - it does nothing if the database already
/// holds films, so it is safe to run on every start-up.
/// </summary>
public static class SeedData
{
    private const int CleaningMinutes = 20;

    // Four hours apart, which leaves room for a film a good deal longer than
    // anything currently seeded, so no two screenings in the same auditorium
    // can overlap.
    private static readonly int[] StartHours = [10, 14, 18, 22];

    public static async Task SeedAsync(ScreenTimeDbContext db)
    {
        if (await db.Movies.AnyAsync())
        {
            return;
        }

        var standard = new SeatType { Name = "Standard", Price = 120m };
        var vip = new SeatType { Name = "VIP", Price = 180m };
        db.SeatTypes.AddRange(standard, vip);

        var genres = new[] { "Supernatural", "Psychological", "Slasher", "Sci-Fi" }
            .ToDictionary(name => name, name => new Genre { Name = name });
        db.Genres.AddRange(genres.Values);

        var movies = new[]
        {
            new Movie
            {
                Title = "Exorcisten",
                AgeRating = AgeRating.Age11,
                RuntimeMinutes = 122,
                Description = "En ung flicka drabbas av en skrämmande, oförklarlig åkomma. När läkare och psykiatriker misslyckas vänder sig hennes desperata mor till kyrkan som sista utväg.",
                Poster = "Exorcisten.png"
            },
            new Movie
            {
                Title = "Hereditary",
                AgeRating = AgeRating.Age15,
                RuntimeMinutes = 126,
                Description = "Efter matriarken Ellens död avslöjas kryptiska hemligheter om familjen Grahams historia, hemligheter som drar dem allt närmare ett olycksaligt öde.",
                Poster = "Hereditary.png"
            },
            new Movie
            {
                Title = "The Shining",
                AgeRating = AgeRating.Age15,
                RuntimeMinutes = 120,
                Description = "Jack Torrance tar jobb som vintervaktmästare på det isolerade hotellet Overlook tillsammans med sin fru och son. Men hotellet döljer en ond och mörk hemlighet.",
                Poster = "TheShining.png"
            },
            new Movie
            {
                Title = "Psycho",
                AgeRating = AgeRating.Age7,
                RuntimeMinutes = 109,
                Description = "På flykt efter en stöld svänger Marion Crane förbi ett ödsligt motell som drivs av den blyge Norman Bates och hans mor, en man med en mörk hemlighet.",
                Poster = "Psycho.png"
            },
            new Movie
            {
                Title = "The Thing",
                AgeRating = AgeRating.Age11,
                RuntimeMinutes = 109,
                Description = "På en isolerad forskningsstation i Antarktis väcker en grupp forskare något främmande till liv som legat begravt i isen i 100 000 år, med förödande konsekvenser.",
                Poster = "TheThing.png"
            },
            new Movie
            {
                Title = "Casper",
                AgeRating = AgeRating.AllAges,
                RuntimeMinutes = 101,
                Description = "Spökdoktorn James Harvey och hans dotter Kat flyttar in i slottet Whipstaff Manor, där det vänliga spöket Casper längtar efter en vän, men hans stökiga farbröder har andra planer.",
                Poster = "Casper.png"
            },
            new Movie
            {
                Title = "The Conjuring",
                AgeRating = AgeRating.Age11,
                RuntimeMinutes = 112,
                Description = "De paranormala utredarna Ed och Lorraine Warren kallas in för att hjälpa en familj som terroriseras av en okänd kraft i sitt hus på landsbygden.",
                Poster = "TheConjuring.png"
            },
            new Movie
            {
                Title = "Alien",
                AgeRating = AgeRating.Age15,
                RuntimeMinutes = 117,
                Description = "Besättningen på rymdskeppet Nostromo får en nödsignal från en närliggande planet. Vad de möter där är en mardröm ingen av dem var förberedd på.",
                Poster = "Alien.png"
            },
            new Movie
            {
                Title = "Alla Helgons Blodiga Natt",
                AgeRating = AgeRating.Age15,
                RuntimeMinutes = 91,
                Description = "1963 mördar sexårige Michael Myers sin syster och spärras in. 15 år senare rymmer han och återvänder till Haddonfield för att mörda igen.",
                Poster = "AllaHelgonsBlodigaNatt.png"
            },
            new Movie
            {
                Title = "Sinister",
                AgeRating = AgeRating.Age15,
                RuntimeMinutes = 110,
                Description = "En författare som utreder ett brutalt mord hittar gamla inspelningar i sitt nya hus, en upptäckt som gör honom och hans familj till mål för ett övernaturligt väsen.",
                Poster = "Sinister.png"
            }
        };
        db.Movies.AddRange(movies);

        db.MovieGenres.AddRange(Link(movies[0], "Supernatural"));
        db.MovieGenres.AddRange(Link(movies[1], "Supernatural", "Psychological"));
        db.MovieGenres.AddRange(Link(movies[2], "Supernatural", "Psychological"));
        db.MovieGenres.AddRange(Link(movies[3], "Psychological"));
        db.MovieGenres.AddRange(Link(movies[4], "Sci-Fi", "Slasher"));
        db.MovieGenres.AddRange(Link(movies[5], "Supernatural"));
        db.MovieGenres.AddRange(Link(movies[6], "Supernatural"));
        db.MovieGenres.AddRange(Link(movies[7], "Sci-Fi"));
        db.MovieGenres.AddRange(Link(movies[8], "Slasher"));
        db.MovieGenres.AddRange(Link(movies[9], "Supernatural", "Psychological"));

        var stora = new Auditorium { Name = "Stora salongen", IsActive = true };
        var lilla = new Auditorium { Name = "Lilla salongen", IsActive = true };
        db.Auditoriums.AddRange(stora, lilla);

        // Stora salongen: 8 rows of 12, the back two rows are VIP. 96 seats.
        db.Seats.AddRange(BuildSeats(stora, "ABCDEFGH", seatsPerRow: 12, vipRows: "GH"));
        // Lilla salongen: 5 rows of 8, all standard. 40 seats.
        db.Seats.AddRange(BuildSeats(lilla, "ABCDE", seatsPerRow: 8, vipRows: ""));

        // Two weeks of programme starting today, so a fresh database has
        // something on the home page's "Dagens visningar" straight away. The
        // programme endpoint hides slots that have already passed. "Today" and
        // the start hours are on the cinema's clock; what is stored is UTC.
        var firstDay = CinemaClock.Today;

        // Even-indexed films play in Stora, odd in Lilla: five films each.
        var programmes = new[]
        {
            (Auditorium: stora, Movies: movies.Where((_, i) => i % 2 == 0).ToArray()),
            (Auditorium: lilla, Movies: movies.Where((_, i) => i % 2 == 1).ToArray()),
        };

        for (var day = 0; day < 14; day++)
        {
            foreach (var (auditorium, films) in programmes)
            {
                // Four slots for five films: rotating by day means each film
                // sits out one day in five and never shares a slot with another.
                for (var slot = 0; slot < StartHours.Length; slot++)
                {
                    var movie = films[(day + slot) % films.Length];
                    var start = CinemaClock.FromCinemaTime(
                        firstDay.AddDays(day).ToDateTime(new TimeOnly(StartHours[slot], 0)));

                    db.Screenings.Add(new Screening
                    {
                        Movie = movie,
                        Auditorium = auditorium,
                        StartTime = start,
                        EndTime = start.AddMinutes(movie.RuntimeMinutes + CleaningMinutes),
                        IsCancelled = false
                    });
                }
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
