using ScreenTime.Application.Contracts;
using ScreenTime.Application.Interfaces;

namespace ScreenTime.Application.Services;

public class SeatService : ISeatService
{
    private readonly ISeatRepository _seatRepository;

    public SeatService(ISeatRepository seatRepository)
    {
        _seatRepository = seatRepository;
    }

    public async Task<List<SeatDto>> GetAllSeatsAsync()
    {
        var seats = await _seatRepository.GetAllAsync();
        return seats.Select(s => new SeatDto(
            s.Id,
            s.Auditorium.Name,
            s.RowLabel,
            s.SeatNumber,
            s.SeatType.Name
        )).ToList();
    }
}