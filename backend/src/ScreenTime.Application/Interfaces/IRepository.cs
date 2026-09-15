using ScreenTime.Domain.Common;

namespace ScreenTime.Application.Interfaces;

/// <summary>
/// The data operations every entity repository shares.
/// </summary>
/// <typeparam name="TEntity">
/// Constrained to <see cref="BaseEntity"/> so the implementation can rely on a
/// Guid Id being present.
/// </typeparam>
public interface IRepository<TEntity> where TEntity : BaseEntity
{
    // The repository's default view of the entity.
    Task<List<TEntity>> GetAllAsync(
        CancellationToken cancellationToken = default);

    // Track only when the caller intends to change what comes back. The default
    // is a no-tracking read.
    Task<TEntity?> GetByIdAsync(
        Guid id,
        bool track = false,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default);

    Task AddRangeAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default);

    void Update(TEntity entity);

    // True when at least one row was written.
    Task<bool> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
