using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenTime.Application.Contracts;

public record ScreeningDto(
    Guid Id,
    string MovieTitle,
    string AuditoriumName,
    DateTime StartTime,
    DateTime EndTime
);
