using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenTime.Application.Contracts;

public record SeatDto(
    Guid Id,
    string AuditoriumName,
    string RowLabel,
    int SeatNumber,
    string SeatTypeName
);

public record SeatCreateDto(
        [Required, MaxLength(5)] string RowLabel,
        [Range(1, int.MaxValue)] int SeatNumber,
        Guid SeatTypeId
    );

public record AddSeatsToAuditoriumDto(
    Guid AuditoriumId,
    [Required, MinLength(1)] List<SeatCreateDto> Seats
);