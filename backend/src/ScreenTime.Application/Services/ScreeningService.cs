using ScreenTime.Application.Contracts;
using ScreenTime.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenTime.Application.Services;

public class ScreeningService : IScreeningService
{
    private readonly IScreeningRepository _screeningRepository;

    public ScreeningService(IScreeningRepository screeningRepository)
    {
        _screeningRepository = screeningRepository;
    }

    public async Task<List<ScreeningDto>> GetAllScreeningsAsync()
    {
        var screenings = await _screeningRepository.GetAllAsync();
        return screenings.Select(s => new ScreeningDto(
            s.Id,
            s.Movie.Title,
            s.Auditorium.Name,
            s.StartTime,
            s.EndTime
        )).ToList();
    }
}