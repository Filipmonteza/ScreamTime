using System.ComponentModel.DataAnnotations;
using ScreenTime.Domain.Enums;

namespace ScreenTime.Application.Contracts;

// Data Transfer Object for creating a new movie.
public record CreateMovieDto(
    // The title of the movie.
    [Required, MaxLength(100)] string Title,

    // The age rating of the movie.
    AgeRating AgeRating,

    // The runtime of the movie in minutes.
    [Range(1, 600)] int RuntimeMinutes,

    // A description of the movie.
    [Required, MaxLength(1000)] string Description,

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
    bool IsActive
);