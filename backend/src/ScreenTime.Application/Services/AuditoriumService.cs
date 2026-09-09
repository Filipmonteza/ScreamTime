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

    public async Task<List<AuditoriumListDto>> GetAllAuditoriumsAsync(
        bool includeInactive = false)
    {
        var auditoriums = await _auditoriumRepository.GetAllAsync(
            includeInactive);

        var auditoriumDtos = new List<AuditoriumListDto>();

        foreach (var auditorium in auditoriums)
        {
            var capacity = await _auditoriumRepository.GetCapacityAsync(
                auditorium.Id);

            auditoriumDtos.Add(new AuditoriumListDto(
                auditorium.Id,
                auditorium.Name,
                capacity,
                auditorium.IsActive
            ));
        }

        return auditoriumDtos;
    }

    public async Task<AuditoriumDetailsDto?> GetAuditoriumByIdAsync(Guid id, bool includeInactive = false)
    {
        var auditorium = await _auditoriumRepository.GetByIdAsync(
            id,
            includeInactive);
        if (auditorium is null)
            return null;
        var seats = await _auditoriumRepository.GetSeatsByAuditoriumIdAsync(auditorium.Id);

        var seatDtos = seats
            .OrderBy(seat => seat.RowLabel)
            .ThenBy(seat => seat.SeatNumber)
            .Select(seat => new AuditoriumSeatDto(
                seat.Id,
                seat.RowLabel,
                seat.SeatNumber,
                seat.SeatType.Name
            ))
            .ToList();

        return new AuditoriumDetailsDto(
            auditorium.Id,
            auditorium.Name,
            seats.Count,
            auditorium.IsActive,
            seatDtos
            );
    }

    public async Task<bool?> UpdateAuditoriumAsync(Guid id, AuditoriumUpdateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return null;

        var name = dto.Name.Trim();

        if (name.Length > 50)
            return null;

        var auditorium = await _auditoriumRepository.GetByIdAsync(
            id,
            includeInactive: true,
            track: true);

        if (auditorium is null)
            return false;

        if (await _auditoriumRepository.AuditoriumNameExistsAsync(
                name,
                auditorium.Id))
            return null;

        auditorium.Name = name;
        auditorium.IsActive = dto.IsActive;

        _auditoriumRepository.Update(auditorium);

        return await _auditoriumRepository.SaveChangesAsync();
    }

    public async Task<bool> DeleteAuditoriumAsync(Guid id)
    {
        var auditorium = await _auditoriumRepository.GetByIdAsync(
            id,
            track: true);

        if (auditorium is null)
            return false;

        auditorium.IsActive = false;

        _auditoriumRepository.Update(auditorium);

        return await _auditoriumRepository.SaveChangesAsync();
    }
}