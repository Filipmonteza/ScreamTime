using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenTime.Application.Contracts;

public record GenreDto(
    Guid Id, 
    string Name
    );
