using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenTime.Application.Contracts;

public record SeatCreateDto(
        [Required, MaxLength(5)] string RowLabel,
        [Range(1, int.MaxValue)] int SeatNumber,
        Guid SeatTypeId
);

public record AddSeatsToAuditoriumDto(
    Guid AuditoriumId,
    [Required, MinLength(1)] List<SeatCreateDto> Seats
);

public record AddSeatsRequestDto(
    [Required, MinLength(1)] List<SeatCreateDto> Seats
);

public record SeatTypeDto(
    Guid Id, 
    string Name, 
    decimal Price
);