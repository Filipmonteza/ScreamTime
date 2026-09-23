using ScreenTime.Application.Contracts;
using ScreenTime.Application.Interfaces;
using ScreenTime.Domain.Entities;

namespace ScreenTime.Application.Services;

public class SeatService : ISeatService
{
    private readonly ISeatRepository _seatRepository;
    private readonly IAuditoriumRepository _auditoriumRepository;

    public SeatService(
        ISeatRepository seatRepository,
        IAuditoriumRepository auditoriumRepository)
    {
        _seatRepository = seatRepository;
        _auditoriumRepository = auditoriumRepository;
    }

    public async Task<List<SeatTypeDto>> GetAllSeatTypesAsync(
        CancellationToken cancellationToken = default)
    {
        var seatTypes = await _seatRepository.GetAllSeatTypesAsync(cancellationToken);
        return seatTypes.Select(st => new SeatTypeDto(
            st.Id, 
            st.Name, 
            st.Price
            )).ToList();
    }

    public async Task AddSeatsToAuditoriumAsync(
        AddSeatsToAuditoriumDto dto)
    {
        // Reject invalid or empty seat batches before querying the database.
        if (dto.AuditoriumId == Guid.Empty ||
            dto.Seats is null ||
            dto.Seats.Count == 0)
            throw new ArgumentException("Ogiltig eller tom sätesdata.");

        var auditorium = await _auditoriumRepository.GetByIdAsync(
            dto.AuditoriumId);

        if (auditorium is null)
            throw new KeyNotFoundException("Salongen hittades inte.");

        if (dto.Seats.Any(seat =>
                string.IsNullOrWhiteSpace(seat.RowLabel) ||
                seat.RowLabel.Trim().Length > 5 ||
                seat.SeatNumber < 1 ||
                seat.SeatTypeId == Guid.Empty))
            throw new ArgumentException("Ogiltig sätesdata.");

        // Normalize row labels so seat coordinates are compared case-insensitively.
        var normalizedSeats = dto.Seats
            .Select(seat => seat with
            {
                RowLabel = seat.RowLabel.Trim().ToUpperInvariant()
            })
            .ToList();

        // Reject duplicate seat coordinates within the incoming batch.
        var uniqueSeatCount = normalizedSeats
            .Select(seat => (seat.RowLabel, seat.SeatNumber))
            .Distinct()
            .Count();

        if (uniqueSeatCount != normalizedSeats.Count)
            throw new ArgumentException("Dubbletter i sätesdata.");

        // Reject seat coordinates that already exist in this auditorium.
        var existingSeats = await _seatRepository.GetByAuditoriumIdAsync(
            dto.AuditoriumId);

        var existingSeatKeys = existingSeats
            .Select(seat => (
                RowLabel: seat.RowLabel.Trim().ToUpperInvariant(),
                seat.SeatNumber))
            .ToHashSet();

        var conflictsWithExistingSeats = normalizedSeats.Any(seat =>
            existingSeatKeys.Contains((
                seat.RowLabel,
                seat.SeatNumber)));

        if (conflictsWithExistingSeats)
            throw new ArgumentException("Ett eller flera säten finns redan i salongen.");

        // Ensure every requested seat type exists before creating any seats.
        var requestedSeatTypeIds = normalizedSeats
            .Select(seat => seat.SeatTypeId)
            .Distinct()
            .ToList();

        var existingSeatTypes = await _seatRepository.GetSeatTypesByIdsAsync(
            requestedSeatTypeIds);

        if (existingSeatTypes.Count != requestedSeatTypeIds.Count)
            throw new ArgumentException("Okänd sätestyp.");

        // Create and save the entire seat batch as one operation.
        var seatsToAdd = normalizedSeats
            .Select(seat => new Seat
            {
                Id = Guid.NewGuid(),
                AuditoriumId = dto.AuditoriumId,
                RowLabel = seat.RowLabel,
                SeatNumber = seat.SeatNumber,
                SeatTypeId = seat.SeatTypeId
            })
            .ToList();

        await _seatRepository.AddRangeAsync(seatsToAdd);

        await _seatRepository.SaveChangesAsync();
    }
}
