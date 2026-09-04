using System;
using System.Collections.Generic;
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