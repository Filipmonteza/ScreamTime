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

    public class SeatRepository : ISeatRepository
{
    private readonly ScreenTimeDbContext _context;

    public SeatRepository(ScreenTimeDbContext context)
    {
        _context = context;
    }

    public async Task<List<Seat>> GetAllAsync() =>
    await _context.Seats
        .Include(s => s.Auditorium)
        .Include(s => s.SeatType)
        .AsNoTracking()
        .ToListAsync();
}