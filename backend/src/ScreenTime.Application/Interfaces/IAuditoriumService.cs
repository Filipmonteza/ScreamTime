using ScreenTime.Application.Contracts;

namespace ScreenTime.Application.Interfaces;

public interface IAuditoriumService
{
    // Create a new auditorium and return its DTO, or null if validation fails.
    Task<AuditoriumDto> CreateAuditoriumAsync(AuditoriumCreateDto dto);

    // Retrieve all auditoriums, optionally including inactive ones for admin views.
    Task<List<AuditoriumListDto>> GetAllAuditoriumsAsync(bool includeInactive = false);

    // Fetch detailed auditorium information, with optional access to inactive records.
    Task<AuditoriumDetailsDto> GetAuditoriumByIdAsync(Guid id, bool includeInactive = false);

    // Update an auditorium and return true, false, or null depending on outcome and validation.
    Task UpdateAuditoriumAsync(Guid id, AuditoriumUpdateDto dto);

    // Soft-delete an auditorium by marking it as inactive.
    Task DeleteAuditoriumAsync(Guid id);
}