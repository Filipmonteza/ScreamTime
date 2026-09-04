using ScreenTime.Application.Contracts;

namespace ScreenTime.Application.Interfaces;

public interface ISeatService
{
    Task<List<SeatDto>> GetAllSeatsAsync();
}