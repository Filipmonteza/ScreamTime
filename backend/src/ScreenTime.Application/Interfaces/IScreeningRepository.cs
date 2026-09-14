using ScreenTime.Domain.Entities;

namespace ScreenTime.Application.Interfaces;

// Reads only until the screening flow is built; GetAllAsync leaves out
// cancelled screenings.
public interface IScreeningRepository : IRepository<Screening>
{
}
