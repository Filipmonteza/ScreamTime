using ScreenTime.Application.Contracts;

namespace ScreenTime.Application.Interfaces;

public interface ISeatService
{
    Task<List<SeatDto>> GetAllSeatsAsync();

    Task<bool?> AddSeatsToAuditoriumAsync(
        AddSeatsToAuditoriumDto dto);

    Task<List<SeatTypeDto>> GetAllSeatTypesAsync(CancellationToken cancellationToken = default);
}