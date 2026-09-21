using System.ComponentModel.DataAnnotations;
using ScreenTime.Domain.Enums;

namespace ScreenTime.Application.Contracts;

// Data Transfer Object for creating a new movie.
public record CreateMovieDto(
    // The title of the movie.
    [Required(ErrorMessage = "Titel krävs."), MaxLength(100, ErrorMessage = "Titeln får max vara 100 tecken.")] string Title,

    // The age rating of the movie.
    AgeRating AgeRating,

    // The runtime of the movie in minutes.
    [Range(1, 600, ErrorMessage = "Speltiden måste vara mellan 1 och 600 minuter.")] int RuntimeMinutes,

    // A description of the movie.
    [Required(ErrorMessage = "Beskrivning krävs."), MaxLength(1000, ErrorMessage = "Beskrivningen får max vara 1000 tecken.")] string Description,

    // The genres associated with the movie.
    List<Guid> GenreIds
);

// Data Transfer Object for returning movie information.
public record MovieDto(
    Guid MovieId,
    string Title,
    AgeRating AgeRating,
    int RuntimeMinutes,
    string Description,
    bool IsActive,
    List<string> Genres,
    string? Poster
);

// Data Transfer Object for updating an existing movie.
public record UpdateMovieDto(
    [Required(ErrorMessage = "Titel krävs."), MaxLength(100, ErrorMessage = "Titeln får max vara 100 tecken.")] string Title,
    AgeRating AgeRating,
    [Range(1, 600, ErrorMessage = "Speltiden måste vara mellan 1 och 600 minuter.")] int RuntimeMinutes,
    [Required(ErrorMessage = "Beskrivning krävs."), MaxLength(1000, ErrorMessage = "Beskrivningen får max vara 1000 tecken.")] string Description,
    bool IsActive,
    List<Guid> GenreIds
);