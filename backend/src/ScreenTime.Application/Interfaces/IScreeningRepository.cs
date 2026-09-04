using ScreenTime.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenTime.Application.Interfaces;

public interface IScreeningRepository
{
    Task<List<Screening>> GetAllAsync();
}