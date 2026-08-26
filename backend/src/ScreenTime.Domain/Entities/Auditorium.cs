using ScreenTime.Domain.Common;

namespace ScreenTime.Domain.Entities;

public class Auditorium : Entity
{
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}