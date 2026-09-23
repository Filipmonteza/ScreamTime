using ScreenTime.Application.Contracts;

namespace ScreenTime.Application.Interfaces;

public interface ISeatService
{
    Task AddSeatsToAuditoriumAsync(
        AddSeatsToAuditoriumDto dto);

    Task<List<SeatTypeDto>> GetAllSeatTypesAsync(CancellationToken cancellationToken = default);
}