using ScreenTime.Application.Contracts;

namespace ScreenTime.Application.Interfaces;

public interface IScreeningService
{
    Task<List<ScreeningDto>> GetAllScreeningsAsync(); 
}