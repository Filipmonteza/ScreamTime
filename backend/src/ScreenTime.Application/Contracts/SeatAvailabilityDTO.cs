namespace ScreenTime.Application.Contracts;

// Represents one seat and its current availability
public record SeatAvailabilityDto(
    Guid SeatId,
    string RowLabel,
    int SeatNumber,
    string SeatType,
    decimal Price,
    bool IsAvailable
);

// Represents the complete seat map for a screening
public record ScreeningSeatMapDto(
    Guid ScreeningId,
    string MovieTitle,
    DateTime ScreeningStartTime,
    string AuditoriumName,
    IReadOnlyList<SeatAvailabilityDto> Seats
);