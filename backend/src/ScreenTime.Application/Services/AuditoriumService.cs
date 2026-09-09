using ScreenTime.Application.Contracts;
using ScreenTime.Application.Interfaces;
using ScreenTime.Domain.Entities;

namespace ScreenTime.Application.Services;

public class AuditoriumService : IAuditoriumService
{
    private readonly IAuditoriumRepository _auditoriumRepository;

    public AuditoriumService(IAuditoriumRepository auditoriumRepository)
    {
        _auditoriumRepository = auditoriumRepository;
    }

    public async Task<AuditoriumDto?> CreateAuditoriumAsync(
        AuditoriumCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return null;

        var name = dto.Name.Trim();

        if (name.Length > 50)
            return null;

        if (await _auditoriumRepository.AuditoriumNameExistsAsync(name))
            return null;

        var auditorium = new Auditorium
        {
            Id = Guid.NewGuid(),
            Name = name,
            IsActive = true
        };

        await _auditoriumRepository.AddAsync(auditorium);

        if (!await _auditoriumRepository.SaveChangesAsync())
            return null;

        return new AuditoriumDto(
            auditorium.Id,
            auditorium.Name,
            auditorium.IsActive
        );
    }
}