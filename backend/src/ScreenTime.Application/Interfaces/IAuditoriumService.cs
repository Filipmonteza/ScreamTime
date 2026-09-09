using ScreenTime.Application.Contracts;

namespace ScreenTime.Application.Interfaces;

public interface IAuditoriumService
{
    Task<AuditoriumDto?> CreateAuditoriumAsync(AuditoriumCreateDto dto);
    Task<List<AuditoriumListDto>> GetAllAuditoriumsAsync(bool includeInactive = false);
    Task<AuditoriumDetailsDto?> GetAuditoriumByIdAsync(
        Guid id,
        bool includeInactive = false);
    Task<bool?> UpdateAuditoriumAsync(
        Guid id,
        AuditoriumUpdateDto dto);
}