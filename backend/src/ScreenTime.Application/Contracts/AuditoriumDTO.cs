using System.ComponentModel.DataAnnotations;

namespace ScreenTime.Application.Contracts;

public record AuditoriumCreateDto(
    [Required(ErrorMessage = "Salongsnamn krävs."), MaxLength(50, ErrorMessage = "Namnet får max vara 50 tecken.")] string Name
);

public record AuditoriumDto(
    Guid AuditoriumId,
    string Name,
    bool IsActive
);

public record AuditoriumListDto(
    Guid AuditoriumId,
    string Name,
    int Capacity,
    bool IsActive
);

public record AuditoriumSeatDto(
    Guid SeatId,
    string RowLabel,
    int SeatNumber,
    string SeatTypeName
);

public record AuditoriumDetailsDto(
    Guid AuditoriumId,
    string Name,
    int Capacity,
    bool IsActive,
    List<AuditoriumSeatDto> Seats
);

public record AuditoriumUpdateDto(
    [Required(ErrorMessage = "Salongsnamn krävs."), MaxLength(50, ErrorMessage = "Namnet får max vara 50 tecken.")] string Name,
    bool IsActive
);