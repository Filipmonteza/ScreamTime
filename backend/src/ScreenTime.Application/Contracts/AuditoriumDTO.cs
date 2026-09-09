using System.ComponentModel.DataAnnotations;

namespace ScreenTime.Application.Contracts;

public record AuditoriumCreateDto(
    [Required, MaxLength(50)] string Name
);

public record AuditoriumDto(
    Guid AuditoriumId,
    string Name,
    bool IsActive
);