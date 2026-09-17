using ScreenTime.Application.Contracts;
using ScreenTime.Application.Interfaces;
using ScreenTime.Domain.Entities;

namespace ScreenTime.Application.Services;

public class AuditoriumService : IAuditoriumService
{
    private readonly IAuditoriumRepository _auditoriumRepository;
    private readonly ISeatRepository _seatRepository;

    // Inject repository to access auditorium persistence and validation rules.
    public AuditoriumService(
        IAuditoriumRepository auditoriumRepository,
        ISeatRepository seatRepository)
    {
        _auditoriumRepository = auditoriumRepository;
        _seatRepository = seatRepository;
    }

    // Create a new auditorium when name is valid, unique and within allowed limits.
    public async Task<AuditoriumDto> CreateAuditoriumAsync(AuditoriumCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Salongsnamn krävs.");

        var name = dto.Name.Trim();

        if (name.Length > 50)
            throw new ArgumentException("Namnet får max vara 50 tecken.");

        if (await _auditoriumRepository.AuditoriumNameExistsAsync(name))
            throw new ArgumentException("Det finns redan en salong med det namnet.");

        var auditorium = new Auditorium
        {
            Id = Guid.NewGuid(),
            Name = name,
            IsActive = true
        };

        await _auditoriumRepository.AddAsync(auditorium);
        await _auditoriumRepository.SaveChangesAsync();

        return new AuditoriumDto(
            auditorium.Id,
            auditorium.Name,
            auditorium.IsActive
        );
    }

    // Retrieve all auditoriums and include capacity for listing views.
    public async Task<List<AuditoriumListDto>> GetAllAuditoriumsAsync(bool includeInactive = false)
    {
        var auditoriums = await _auditoriumRepository.GetAllAsync(includeInactive);

        var auditoriumIds = auditoriums.Select(a => a.Id).ToList();
        var capacities = await _auditoriumRepository.GetCapacitiesAsync(auditoriumIds);

        return auditoriums
            .Select(auditorium => new AuditoriumListDto(
                auditorium.Id,
                auditorium.Name,
                capacities.GetValueOrDefault(auditorium.Id, 0),
                auditorium.IsActive
            ))
            .ToList();
    }

    // Fetch full auditorium details including ordered seat map.
    public async Task<AuditoriumDetailsDto> GetAuditoriumByIdAsync(
        Guid id,
        bool includeInactive = false)
    {
        var auditorium = await _auditoriumRepository.GetByIdAsync(
            id,
            includeInactive: includeInactive) ?? throw new KeyNotFoundException("Salongen hittades inte.");

        var seats = await _seatRepository.GetByAuditoriumIdAsync(
            auditorium.Id);

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

    // Update auditorium name and active status when validation and uniqueness checks pass.
    public async Task UpdateAuditoriumAsync(Guid id, AuditoriumUpdateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Salongsnamn krävs.");

        var name = dto.Name.Trim();

        if (name.Length > 50)
            throw new ArgumentException("Namnet får max vara 50 tecken.");

        var auditorium = await _auditoriumRepository.GetByIdAsync(
            id,
            includeInactive: true,
            track: true);

        if (auditorium is null)
            throw new KeyNotFoundException("Salongen hittades inte.");

        if (await _auditoriumRepository.AuditoriumNameExistsAsync(name, auditorium.Id))
            throw new ArgumentException("Det finns redan en salong med det namnet.");

        auditorium.Name = name;
        auditorium.IsActive = dto.IsActive;

        _auditoriumRepository.Update(auditorium);

        await _auditoriumRepository.SaveChangesAsync();
    }

    // Soft-delete an auditorium by marking it inactive.
    // Note: does not currently check for future showtimes.
    public async Task DeleteAuditoriumAsync(Guid id)
    {
        var auditorium = await _auditoriumRepository.GetByIdAsync(id, includeInactive: true, track: true);

        if (auditorium is null)
            throw new KeyNotFoundException("Salongen hittades inte.");

        auditorium.IsActive = false;

        _auditoriumRepository.Update(auditorium);

        await _auditoriumRepository.SaveChangesAsync();
    }
}