using ScreenTime.Application.Contracts;

namespace ScreenTime.Application.Interfaces;

public interface IAuditoriumService
{
    // Create a new auditorium and return its DTO. Throws ArgumentException if
    // the name is missing or already taken.
    Task<AuditoriumDto> CreateAuditoriumAsync(AuditoriumCreateDto dto);

    // Retrieve all auditoriums, optionally including inactive ones for admin views.
    Task<List<AuditoriumListDto>> GetAllAuditoriumsAsync(bool includeInactive = false);

    // Fetch detailed auditorium information, with optional access to inactive records.
    Task<AuditoriumDetailsDto> GetAuditoriumByIdAsync(Guid id, bool includeInactive = false);

    // Update an auditorium's name and active flag. Throws KeyNotFoundException
    // if it does not exist, ArgumentException if the name is missing or taken,
    // and InvalidOperationException if it is being deactivated while screenings
    // there have yet to start.
    Task UpdateAuditoriumAsync(Guid id, AuditoriumUpdateDto dto);

    // Soft-delete an auditorium by marking it as inactive. Throws
    // InvalidOperationException if screenings there have yet to start.
    Task DeleteAuditoriumAsync(Guid id);
}