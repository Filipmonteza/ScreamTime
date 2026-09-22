using ScreenTime.Application.Contracts;
using ScreenTime.Application.Interfaces;
using ScreenTime.Domain.Entities;

namespace ScreenTime.Application.Services;

public class AuditoriumService : IAuditoriumService
{
    private readonly IAuditoriumRepository _auditoriumRepository;
    private readonly ISeatRepository _seatRepository;
    private readonly IScreeningRepository _screeningRepository;

    // Inject repository to access auditorium persistence and validation rules.
    // The screenings are here for one rule: an auditorium with screenings still
    // to come cannot be deactivated.
    public AuditoriumService(
        IAuditoriumRepository auditoriumRepository,
        ISeatRepository seatRepository,
        IScreeningRepository screeningRepository)
    {
        _auditoriumRepository = auditoriumRepository;
        _seatRepository = seatRepository;
        _screeningRepository = screeningRepository;
    }

    // Create a new auditorium when name is valid, unique and within allowed limits.
    public async Task<AuditoriumDto> CreateAuditoriumAsync(AuditoriumCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Salongsnamn krävs.");

        var name = dto.Name.Trim();

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

        var auditorium = await _auditoriumRepository.GetByIdAsync(
            id,
            includeInactive: true,
            track: true);

        if (auditorium is null)
            throw new KeyNotFoundException("Salongen hittades inte.");

        if (await _auditoriumRepository.AuditoriumNameExistsAsync(name, auditorium.Id))
            throw new ArgumentException("Det finns redan en salong med det namnet.");

        // Only the transition is guarded: renaming an auditorium that is already
        // inactive must not trip over the screenings it once held.
        if (auditorium.IsActive && !dto.IsActive)
            await EnsureNoUpcomingScreeningsAsync(auditorium.Id);

        auditorium.Name = name;
        auditorium.IsActive = dto.IsActive;

        _auditoriumRepository.Update(auditorium);

        await _auditoriumRepository.SaveChangesAsync();
    }

    // Soft-delete an auditorium by marking it inactive, once it has no screenings
    // left to come.
    public async Task DeleteAuditoriumAsync(Guid id)
    {
        var auditorium = await _auditoriumRepository.GetByIdAsync(id, includeInactive: true, track: true);

        if (auditorium is null)
            throw new KeyNotFoundException("Salongen hittades inte.");

        if (auditorium.IsActive)
            await EnsureNoUpcomingScreeningsAsync(auditorium.Id);

        auditorium.IsActive = false;

        _auditoriumRepository.Update(auditorium);

        await _auditoriumRepository.SaveChangesAsync();
    }

    // Deactivating is a flag on the auditorium alone and does not reach its
    // screenings, which would go on showing in the programme and taking bookings
    // for an auditorium that is no longer running. So the screenings have to be
    // cancelled first - that is what cancels the bookings sold for them and
    // releases the seats, so the customers' bookings show as cancelled instead of
    // pointing at a show that is not happening - and only then can the auditorium
    // be deactivated.
    //
    // A screening that has already begun does not stand in the way: nobody is
    // shown out mid-film, and from the moment it starts it is off the programme
    // and unbookable anyway, so it simply plays itself out.
    private async Task EnsureNoUpcomingScreeningsAsync(Guid auditoriumId)
    {
        if (await _screeningRepository.HasUpcomingScreeningsAsync(auditoriumId))
        {
            throw new InvalidOperationException(
                "Salongen har kommande visningar och kan därför inte inaktiveras. " +
                "Avboka visningarna först.");
        }
    }
}