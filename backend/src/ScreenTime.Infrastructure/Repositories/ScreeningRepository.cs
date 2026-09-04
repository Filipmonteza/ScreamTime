using Microsoft.EntityFrameworkCore;
using ScreenTime.Application.Interfaces;
using ScreenTime.Domain.Entities;
using ScreenTime.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenTime.Infrastructure.Repositories;


public class ScreeningRepository : IScreeningRepository
{
    private readonly ScreenTimeDbContext _context;

    public ScreeningRepository(ScreenTimeDbContext context)
    {
        _context = context;
    }

    public async Task<List<Screening>> GetAllAsync() =>
    await _context.Screenings
        .Include(s => s.Movie)
        .Include(s => s.Auditorium)
        .Where(s => !s.IsCancelled)
        .AsNoTracking()
        .ToListAsync();
}