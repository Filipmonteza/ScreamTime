using ScreenTime.Domain.Entities;

namespace ScreenTime.Application.Interfaces;

public interface IAuditoriumRepository
{
    // Retrieve all auditoriums. Optional flag allows inclusion of inactive ones
    // so administrative views can show full history or allow reactivation.
    Task<List<Auditorium>> GetAllAsync(
        bool includeInactive = false);

    // Fetch a single auditorium by ID. Optional flags:
    // - includeInactive: needed when editing or inspecting archived salonger.
    // - track: disabled by default to avoid unnecessary change tracking in read-only flows.
    Task<Auditorium?> GetByIdAsync(
        Guid id,
        bool includeInactive = false,
        bool track = false);

    // Retrieve all seats belonging to a specific auditorium.
    // Used for capacity checks, seat map generation, and validation before updates.
    Task<List<Seat>> GetSeatsByAuditoriumIdAsync(
        Guid auditoriumId);

    // Return the auditorium's capacity. This is calculated from seats rather than stored,
    // ensuring consistency even after seat updates or reconfiguration.
    Task<int> GetCapacityAsync(Guid auditoriumId);

    // Check if an auditorium name already exists. Optional exclusion parameter allows
    // renaming an existing auditorium without triggering false positives.
    Task<bool> AuditoriumNameExistsAsync(
        string name,
        Guid? excludeAuditoriumId = null);

    // Add a new auditorium. SaveChanges is intentionally separate so multiple operations
    // can be batched in a single transaction.
    Task AddAsync(Auditorium auditorium);

    // Update an existing auditorium. Caller is responsible for invoking SaveChanges
    // to ensure explicit and predictable persistence behavior.
    void Update(Auditorium auditorium);

    // Persist all pending changes. Returns true only when the underlying database
    // confirms the write operation.
    Task<bool> SaveChangesAsync();
}
