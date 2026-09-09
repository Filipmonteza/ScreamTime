using ScreenTime.Application.Contracts;

namespace ScreenTime.Application.Interfaces;

public interface IAuditoriumService
{
    Task<AuditoriumDto?> CreateAuditoriumAsync(AuditoriumCreateDto dto);
}